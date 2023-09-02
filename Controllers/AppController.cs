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
            var requisicao = new Requisicao();
            requisicao.DataInicio = DateTime.Now;
            requisicao.Status = StatusRequisicao.Pendente;
            requisicao.Id = Guid.NewGuid();

            _context.Requisicoes.Add(requisicao);
            _context.SaveChanges();

            //Mensageria(requisicao.Id.ToString());


            var url = "amqps://peelqnnc:gU-p0eAigyVNJNfNPanQHz4onYx-Oe7u@jackal.rmq.cloudamqp.com/peelqnnc";
            var queueName = "teste2";



            //criar uma instancia de mensageiro
            var mensageiro = new Mensageiro(url, queueName, _context);

            //publicar uma mensagem
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


        private void Mensageria(string requisicaoID)
        {
            // define a URL do servidor RabbitMQ
            var rabbitMqUrl = "amqps://peelqnnc:gU-p0eAigyVNJNfNPanQHz4onYx-Oe7u@jackal.rmq.cloudamqp.com/peelqnnc";

            // cria uma fábrica de conexões com a URL
            var factory = new ConnectionFactory { Uri = new Uri(rabbitMqUrl) };


            // cria uma conexão com o servidor usando a fábrica
            using var connection = factory.CreateConnection();

            // cria um canal de comunicação dentro da conexão
            using var channel = connection.CreateModel();

            // declara uma fila chamada "teste" no servidor, se ela não existir
            channel.QueueDeclare(queue: "teste2",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // converte a string teste em um array de bytes
            var body = Encoding.UTF8.GetBytes(requisicaoID);

            // publica a mensagem na troca vazia ("") com a chave de roteamento "teste"
            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: "teste2",
                                 basicProperties: null,
                                 body: body);


            var consumer = new EventingBasicConsumer(channel);


            var result = channel.QueueDeclarePassive("teste2");
            var messageCount = result.MessageCount;

            consumer.Received += (sender, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var requisicao = _context.Requisicoes.FirstOrDefault(x => x.Id.ToString() == message);


                 requisicao.Status = StatusRequisicao.Processando;

                _context.SaveChanges();
            };

            channel.BasicConsume(queue: "teste2",
                                 autoAck: false,
                                 consumer: consumer);

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
    }
}