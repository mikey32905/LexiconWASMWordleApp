using LexiconWASMWordleApp.Enums;

namespace LexiconWASMWordleApp.Models
{
    public class TileState
    {
        public char Letter { get; set; } = ' ';
        public TileStatus Status { get; set; } = TileStatus.Empty;
        public bool IsRevealing { get; set; }
        public bool IsBouncing { get; set; }
        public bool IsShaking { get; set; }
    }
}
