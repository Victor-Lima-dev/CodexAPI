using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodexAPI.Context;
using CodexAPI.Models;
using CodexAPI.Models.Enums;
using Microsoft.AspNetCore.Mvc;

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

             if(requisicao == null)
             {
                 return NotFound();
             }
             
             return Ok(requisicao.Status);
         }

        


        
    }
}