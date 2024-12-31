using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace events2
{
    class TradeOperation
    {
        public string TraderName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public double Amount { get; set; }
        public double Rate { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"[{Timestamp}] Trader: {TraderName}, Action: {Action}, Amount: {Amount}, Rate: {Rate}";
        }
    }

    class Exchange
    {
        public event Action<double>? RateIncreased;
        public event Action<double>? RateDecreased;

        private double _currentRate;
        public double CurrentRate
        {
            get => _currentRate;
            set
            {
                if (value > _currentRate)
                    RateIncreased?.Invoke(value);
                else if (value < _currentRate)
                    RateDecreased?.Invoke(value);

                _currentRate = value;
            }
        }

        private readonly List<TradeOperation> _history = new();

        public void SaveHistory(string filePath)
        {
            var json = JsonSerializer.Serialize(_history, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public void LoadHistory(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var json = File.ReadAllText(filePath);
            var operations = JsonSerializer.Deserialize<List<TradeOperation>>(json);
            if (operations != null)
                _history.AddRange(operations);
        }

        public void AddOperation(TradeOperation operation)
        {
            _history.Add(operation);
            Console.WriteLine("Operation added: " + operation);
        }
    }

    class Trader
    {
        public string Name { get; set; } = string.Empty;
        public double CurrencyAmount { get; set; }

        public void OnRateIncreased(double newRate)
        {
            Console.WriteLine($"{Name}: Rate increased to {newRate}, considering selling...");
            if (CurrencyAmount > 0)
            {
                Console.WriteLine($"{Name} sells at rate {newRate}");
                CurrencyAmount -= 100;
            }
        }

        public void OnRateDecreased(double newRate)
        {
            Console.WriteLine($"{Name}: Rate decreased to {newRate}, considering buying...");
            if (CurrencyAmount > 0)
            {
                Console.WriteLine($"{Name} buys at rate {newRate}");
                CurrencyAmount += 100;
            }
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            string filePath = "history.json";

            Exchange exchange = new Exchange();
            exchange.LoadHistory(filePath);

            Trader alice = new Trader { Name = "Alice", CurrencyAmount = 1000 };
            Trader bob = new Trader { Name = "Bob", CurrencyAmount = 1500 };

            exchange.RateIncreased += alice.OnRateIncreased;
            exchange.RateDecreased += bob.OnRateDecreased;

            exchange.CurrentRate = 1.2;
            exchange.CurrentRate = 1.5;
            exchange.CurrentRate = 1.3;

            exchange.AddOperation(new TradeOperation
            {
                TraderName = "Alice",
                Action = "Sell",
                Amount = 100,
                Rate = 1.5
            });

            exchange.AddOperation(new TradeOperation
            {
                TraderName = "Bob",
                Action = "Buy",
                Amount = 200,
                Rate = 1.3
            });

            exchange.SaveHistory(filePath);

            Console.WriteLine("Simulation completed");
        }
    }
}
