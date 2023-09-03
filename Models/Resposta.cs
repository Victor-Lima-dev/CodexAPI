using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodexAPI.Models
{
    public class Resposta : BaseModel
    {

        
        public Guid  PerguntaId { get; set; }

        public string Conteudo { get; set; }

        public bool Correta { get; set; }

        public string Status { get; set; }
    }
}