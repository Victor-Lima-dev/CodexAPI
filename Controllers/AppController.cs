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
            var queueName = "teste2";

            var mensageiro = new Mensageiro(url, queueName, _context);
            mensageiro.Publicar(requisicao.Id.ToString());

            return Ok(requisicao.Id);

        }

        [HttpGet("VerificarRequisicao")]

        public IActionResult VerificarRequisicao(Guid Id)
        {
            var listaPerguntas = _context.Perguntas.ToList();

            var requisicao = _context.Requisicoes.FirstOrDefault(x => x.Id == Id);

            if (requisicao == null)
            {
                return NotFound();
            }

            return Ok(requisicao.Status);
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

            _context.SaveChanges();

            return Ok(requisicoes);
        }

        private Boolean ValidarTextoBase(TextoBase textoBase)
        {
            return true;
        }

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
                Console.WriteLine("Texto Base Invalido");
                return Ok("Texto Base Invalido");
            }
            else
            {
                requisicao.Status = StatusRequisicao.AguardandoProcessamento;
                _context.SaveChanges();
                Console.WriteLine("Aguardando Processamento");


                var url = "amqps://peelqnnc:gU-p0eAigyVNJNfNPanQHz4onYx-Oe7u@jackal.rmq.cloudamqp.com/peelqnnc";
                var queueName = "Processamento";

                var mensageiro = new Mensageiro(url, queueName, _context);
                mensageiro.Publicar(requisicao.Id.ToString());

                Console.WriteLine("Enviado ao Mensageiro");

                return Ok("Aguardando Processamento");
            }
            
        }


        private Boolean EnviarGPT (TextoBase textoBase)
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
                Console.WriteLine("Processando");
                return Ok("Processando");
            }
        }



    }
}