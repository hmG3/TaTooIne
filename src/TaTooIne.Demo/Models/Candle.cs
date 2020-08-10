using System;

namespace TaTooIne.Demo.Models
{
    public sealed class Candle
    {
        public DateTime Time { get; set; }

        public decimal Open { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Close { get; set; }

        public decimal Volume { get; set; }
    }
}
