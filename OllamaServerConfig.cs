using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticKernelStarter
{
    public class OllamaServerConfig
    {
        public string ServerId { get; set; }
        public string Endpoint { get; set; }
        public string ModelId { get; set; }
        public float Temperature { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime LastUsed { get; set; } = DateTime.MinValue;
    }
}
