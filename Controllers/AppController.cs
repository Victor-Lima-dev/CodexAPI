using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CodexAPI.Context;
using CodexAPI.Models;
using CodexAPI.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CodexAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppController : ControllerBase
    {

        private readonly AppDbContext _context;

        public AppController(AppDbContext context)
        {
            _context = context;
        }



        [HttpGet("EnviarTextoBase")]
        public IActionResult EnviarTextoBase(string TextoBase)
        {
            var requisicao = new Requisicao
            {
                DataInicio = DateTime.Now,
                Status = StatusRequisicao.Pendente,
                Id = Guid.NewGuid()
            };


            //criar texto base
            var textoBase = new TextoBase
            {
                Conteudo = TextoBase,
                RequisicaoId = requisicao.Id
            };

            _context.TextosBase.Add(textoBase);
            _context.Requisicoes.Add(requisicao);
            _context.SaveChanges();


            var url = "amqps://peelqnnc:gU-p0eAigyVNJNfNPanQHz4onYx-Oe7u@jackal.rmq.cloudamqp.com/peelqnnc";
            var queueName = "Pendente";

            var mensageiro = new Mensageiro(url, queueName, _context);
            mensageiro.Publicar(requisicao.Id.ToString());

            return Ok(requisicao.Id);

        }

        [HttpGet("VerificarRequisicao")]

        public IActionResult VerificarStatusRequisicao(Guid Id)
        {
            var listaPerguntas = _context.Perguntas.ToList();

            var requisicao = _context.Requisicoes.FirstOrDefault(x => x.Id == Id);

            if (requisicao == null)
            {
                return NotFound();
            }
            if (requisicao.Status == StatusRequisicao.TextoBaseInvalido)
            {
                //apagar texto base?
                var textoBase = _context.TextosBase.FirstOrDefault(x => x.RequisicaoId == requisicao.Id);
                _context.TextosBase.Remove(textoBase);
                //apagar requisicao?
                _context.Requisicoes.Remove(requisicao);
                _context.SaveChanges();
                return Ok("Requisicao Invalida - Texto Base Invalido, portanto será necessário enviar um novo texto base");
            }

            return Ok(requisicao.Status.ToString());
        }


        [HttpGet("TodasRequisicoes")]
        public IActionResult TodasRequisicoes()
        {
            var requisicoes = _context.Requisicoes.ToList();

            return Ok(requisicoes);
        }



        [HttpGet("Apagar")]
        public IActionResult Apagar()
        {
            //apagar todas as requisicoes
            var requisicoes = _context.Requisicoes.ToList();
            _context.Requisicoes.RemoveRange(requisicoes);

            //apagar todas as perguntas

            var perguntas = _context.Perguntas.ToList();
            _context.Perguntas.RemoveRange(perguntas);

            //apagar todos os textos base
            var textosBase = _context.TextosBase.ToList();
            _context.TextosBase.RemoveRange(textosBase);

            _context.SaveChanges();

            return Ok(requisicoes);
        }


        //Metodo para Validar o Texto Base
        private Boolean ValidarTextoBase(TextoBase textoBase)
        {
            //contar quantas palavras tem no texto base
            var quantidadePalavras = textoBase.Conteudo.Split(" ");

            //se tiver menos que 100 palavras ou mais que 1000, invalidar

            if (quantidadePalavras.Length < 100 || quantidadePalavras.Length > 1000)
            {
                return true;
            }

            return true;
        }


        //Aqui eu recebo da mensageria e tento validar o texto base
        [HttpGet("AguardandoProcessamento")]
        public IActionResult MudarStatus([FromQuery] string message)
        {
            var requisicao = _context.Requisicoes.FirstOrDefault(x => x.Id.ToString() == message);

            if (requisicao == null)
            {
                return NotFound();
            }

            //procurar texto base

            var textoBase = _context.TextosBase.FirstOrDefault(x => x.RequisicaoId == requisicao.Id);

            if (textoBase == null)
            {
                return NotFound();
            }


            var validacaoTextoBase = ValidarTextoBase(textoBase);

            if (validacaoTextoBase == false)
            {
                requisicao.Status = StatusRequisicao.TextoBaseInvalido;
                _context.SaveChanges();
                return Ok("Texto Base Invalido - Por favor insira um texto com mais de 100 palavras e menos de 1000");
            }
            else
            {
                requisicao.Status = StatusRequisicao.AguardandoProcessamento;
                _context.SaveChanges();
                var url = "amqps://peelqnnc:gU-p0eAigyVNJNfNPanQHz4onYx-Oe7u@jackal.rmq.cloudamqp.com/peelqnnc";
                var queueName = "Processamento";
                var mensageiro = new Mensageiro(url, queueName, _context);
                mensageiro.Publicar(requisicao.Id.ToString());
                return Ok("Aguardando Processamento");
            }

        }


        private Boolean EnviarGPT(TextoBase textoBase)
        {
            return true;
        }

        [HttpGet("Processando")]
        public IActionResult Processando([FromQuery] string message)
        {
            var requisicao = _context.Requisicoes.FirstOrDefault(x => x.Id.ToString() == message);

            if (requisicao == null)
            {
                return NotFound();
            }

            //procurar texto base

            var textoBase = _context.TextosBase.FirstOrDefault(x => x.RequisicaoId == requisicao.Id);

            if (textoBase == null)
            {
                return NotFound();
            }


            var validacaoGPT = EnviarGPT(textoBase);

            if (validacaoGPT == false)
            {
                requisicao.Status = StatusRequisicao.FalhaProcessamento;
                _context.SaveChanges();
                Console.WriteLine("Falha Processamento");
                return Ok();
            }
            else
            {
                requisicao.Status = StatusRequisicao.Processando;
                _context.SaveChanges();


                var url = "amqps://peelqnnc:gU-p0eAigyVNJNfNPanQHz4onYx-Oe7u@jackal.rmq.cloudamqp.com/peelqnnc";
                var queueName = "ProcessandoGPT";
                var mensageiro = new Mensageiro(url, queueName, _context);
                mensageiro.Publicar(requisicao.Id.ToString());



                Console.WriteLine("Processando");
                return Ok("Processando");
            }
        }

        //agora vou ter um endpoint para sinalizar que a requisição foi enviada para o gpt

        [HttpGet("ConfirmacaoEnvioGPT")]
        public IActionResult ConfirmacaoEnvioGPT([FromQuery] string message)
        {
            var requisicao = _context.Requisicoes.FirstOrDefault(x => x.Id.ToString() == message);

            if (requisicao == null)
            {
                return NotFound();
            }

            //procurar texto base

            var textoBase = _context.TextosBase.FirstOrDefault(x => x.RequisicaoId == requisicao.Id);

            if (textoBase == null)
            {
                return NotFound();
            }

            requisicao.Status = StatusRequisicao.AguardandoPerguntasRespostas;
            _context.SaveChanges();

            var texto = textoBase.Conteudo;

            return Ok(texto);
        }




        [HttpGet("ReceberJson")]
        public IActionResult ReceberJson(string json, [FromQuery] string requisicao)
        {


            var pergunta = JsonSerializer.Deserialize<Pergunta>(json);




            var confirmarRequisicao = _context.Requisicoes.FirstOrDefault(x => x.Id.ToString() == requisicao);
            var confirmarTextoBase = _context.TextosBase.FirstOrDefault(x => x.RequisicaoId.ToString() == requisicao);

            if (confirmarRequisicao == null || confirmarTextoBase == null)
            {
                return NotFound();
            }

            pergunta.RequisicaoId = confirmarRequisicao.Id;
            //mudar status da pergunta para false
            pergunta.Status = "NãoConferida";

            pergunta.Id = Guid.NewGuid();

            //colocar o id da pergunta na resposta
            foreach (var item in pergunta.Respostas)
            {
                item.PerguntaId = pergunta.Id;
                item.Id = Guid.NewGuid();
                item.Status = "NãoConferida";
            }

            //adicinar a pergunta ao banco de dados
            _context.Perguntas.Add(pergunta);


            confirmarRequisicao.Status = StatusRequisicao.Pronto;
            confirmarRequisicao.DataFim = DateTime.Now;
            _context.SaveChanges();
            return Ok("Recebido");
        }


        [HttpGet("ObterPerguntaRespostas")]
        public IActionResult ObterPerguntaRespostas([FromQuery] string requisicaoRecebida)
        {
            var requisicao = _context.Requisicoes.FirstOrDefault(x => x.Id.ToString() == requisicaoRecebida);

            

            if (requisicao == null)
            {
                return NotFound("Requisicao não encontrada");
            }

            //verificar o status da requisicao, se for diferente de pronto, retornar que ainda não está pronto

            if (requisicao.Status != StatusRequisicao.Pronto)
            {
                return Ok("Ainda não está pronto, Status: " + requisicao.Status.ToString());
            }

            //procurar texto base

            var textoBase = _context.TextosBase.FirstOrDefault(x => x.RequisicaoId == requisicao.Id);

            if (textoBase == null)
            {
                return NotFound("Nenhum texto base referente a esssa requisição foi encontrado");
            }
          
            //procurar a pergunta referente a essa requisição, usar o include para incluir as respostas

            var pergunta = _context.Perguntas.FirstOrDefault(x => x.RequisicaoId == requisicao.Id);

            if (pergunta == null)
            {
                return NotFound("Nenhuma pergunta referente a essa requisição foi encontrada");
            }

            var respostas = _context.Respostas.Where(x => x.PerguntaId == pergunta.Id).ToList();
         
            return Ok(pergunta);
        }

    }
}