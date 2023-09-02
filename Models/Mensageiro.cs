using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodexAPI.Context;
using CodexAPI.Models.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CodexAPI.Models
{
    public class Mensageiro
    {
        // URL do servidor RabbitMQ
        private readonly string _rabbitMqUrl;

        private readonly AppDbContext _context;



        // Nome da fila que será usada
        private readonly string _queueName;

        // Construtor que recebe a URL e o nome da fila como parâmetros
        public Mensageiro(string rabbitMqUrl, string queueName, AppDbContext context)
        {
            _rabbitMqUrl = rabbitMqUrl;
            _queueName = queueName;
            _context = context;
        }

        // Método que publica uma mensagem na fila
        public void Publicar(string mensagem)
        {
            // Cria uma fábrica de conexões com a URL
            var factory = new ConnectionFactory { Uri = new Uri(_rabbitMqUrl) };

            // Cria uma conexão com o servidor usando a fábrica
            using var connection = factory.CreateConnection();

            // Cria um canal de comunicação dentro da conexão
            using var channel = connection.CreateModel();

            // Declara uma fila no servidor, se ela não existir
            channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // Converte a mensagem em um array de bytes
            var body = Encoding.UTF8.GetBytes(mensagem);




            // Publica a mensagem na troca vazia ("") com a chave de roteamento igual ao nome da fila
            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: _queueName,
                                 basicProperties: null,
                                 body: body);
        }


        public void Consumir()
        {
            // Cria uma fábrica de conexões com a URL
            var factory = new ConnectionFactory { Uri = new Uri(_rabbitMqUrl) };

            // Cria uma conexão com o servidor usando a fábrica
            var connection = factory.CreateConnection();

            // Cria um canal de comunicação dentro da conexão
            var channel = connection.CreateModel();

            // Declara uma fila no servidor, se ela não existir
            channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            // Cria um consumidor de eventos para receber as mensagens
            var consumer = new EventingBasicConsumer(channel);

            // Define o evento que será disparado quando uma mensagem for recebida
            consumer.Received += (sender, eventArgs) =>
            {
                // Obtém o corpo da mensagem como um array de bytes
                var body = eventArgs.Body.ToArray();

                // Converte o array de bytes em uma string
                var message = Encoding.UTF8.GetString(body);

                //transformar essa string em guid

                var messageTransformada = Guid.Parse(message);

                //mudar o status da requisição para processando

                var requisicao = _context.Requisicoes.FirstOrDefault(x => x.Id == messageTransformada);

                Console.WriteLine(requisicao.Status);

                requisicao.Status = StatusRequisicao.Processando;

                _context.SaveChanges();

                



              

                // Reconhece a mensagem manualmente depois de processá-la
                channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            // Consome as mensagens da fila sem reconhecê-las automaticamente
            channel.BasicConsume(queue: _queueName,
                                 autoAck: false,
                                 consumer: consumer);
        }


    }
}