using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodexAPI.Models
{
    public class Pergunta : BaseModel
    {
        

        public int RequisicaoId { get; set; }

        public string Conteudo { get; set; }

        public string Status { get; set; }
    }
}