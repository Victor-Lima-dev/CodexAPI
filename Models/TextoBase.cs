using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodexAPI.Models
{
    public class TextoBase : BaseModel
    {       
        public Guid RequisicaoId { get; set; }

        public string Conteudo { get; set; }
    }
}