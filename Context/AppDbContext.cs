using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodexAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodexAPI.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public DbSet<Requisicao> Requisicoes { get; set; }

        public DbSet<Pergunta> Perguntas { get; set; }

        public DbSet<TextoBase> TextosBase { get; set; }

        public DbSet<Resposta> Respostas { get; set; }
    }
}