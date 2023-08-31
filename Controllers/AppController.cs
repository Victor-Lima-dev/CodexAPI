using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    [Route("api/[controller]")]
    public class AppController : ControllerBase
    {

        private readonly AppDbContext _context;

        public AppController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult EnviarTextoBase(string TextoBase)
        {
            var requisicao = new Requisicao();
            requisicao.DataInicio = DateTime.Now;
            requisicao.Status = StatusRequisicao.Pendente;
            requisicao.Id = Guid.NewGuid();

            _context.Requisicoes.Add(requisicao);
            _context.SaveChanges();

            return Ok(requisicao.Id);

        }

        [HttpGet("VerificarRequisicao")]

        public IActionResult VerificarRequisicao(Guid Id)
        {
            var requisicao = _context.Requisicoes.FirstOrDefault(x => x.Id == Id);

            if (requisicao == null)
            {
                return NotFound();
            }

            return Ok(requisicao.Status);
        }

        [HttpGet("TesteMensageria")]

        public IActionResult TesteMensageria(string teste)
        {

            // define a URL do servidor RabbitMQ
            var rabbitMqUrl = "amqp://peelqnnc:gU-p0eAigyVNJNfNPanQHz4onYx-Oe7u@jackal.rmq.cloudamqp.com/peelqnnc";

            // cria uma fábrica de conexões com a URL
            var factory = new ConnectionFactory { Uri = new Uri(rabbitMqUrl) };


            // cria uma conexão com o servidor usando a fábrica
            using var connection = factory.CreateConnection();

            // cria um canal de comunicação dentro da conexão
            using var channel = connection.CreateModel();

            // declara uma fila chamada "teste" no servidor, se ela não existir
            channel.QueueDeclare(queue: "teste",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // converte a string teste em um array de bytes
            var body = Encoding.UTF8.GetBytes(teste);

            // publica a mensagem na troca vazia ("") com a chave de roteamento "teste"
            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: "teste",
                                 basicProperties: null,
                                 body: body);

            // fecha o canal e a conexão
            channel.Close();
            connection.Dispose();

            // retorna um resultado da API, por exemplo, um status 200 (OK)
            return Ok();


        }


        [HttpGet("TesteMensageria2")]
        public IActionResult Teste2 ()
        {

             // define a URL do servidor RabbitMQ
            var rabbitMqUrl = "amqp://peelqnnc:gU-p0eAigyVNJNfNPanQHz4onYx-Oe7u@jackal.rmq.cloudamqp.com/peelqnnc";

            // cria uma fábrica de conexões com a URL
            var factory = new ConnectionFactory { Uri = new Uri(rabbitMqUrl) };


            // cria uma conexão com o servidor usando a fábrica
            using var connection = factory.CreateConnection();

            // cria um canal de comunicação dentro da conexão
            using var channel = connection.CreateModel();

            // declara uma fila chamada "teste" no servidor, se ela não existir
            channel.QueueDeclare(queue: "teste",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
            };

            channel.BasicConsume(queue: "teste",
                                 autoAck: true,
                                 consumer: consumer);

            return Ok();
        }


    }
}