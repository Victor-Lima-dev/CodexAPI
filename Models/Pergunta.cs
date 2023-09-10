using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CodexAPI.Models
{
    public class Pergunta : BaseModel
    {


        public Guid RequisicaoId { get; set; }

        public string Conteudo { get; set; }

        public string Status { get; set; }

        public List<Resposta> Respostas { get; set; } = new List<Resposta>();


      
    }


}