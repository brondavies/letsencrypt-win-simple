﻿using Newtonsoft.Json;
using System;

namespace LetsEncrypt.ACME.Simple
{
    internal class ScheduledRenewal
    {
        public DateTime Date { get; set; }
        public Target Binding { get; set; }
        public string CentralSsl { get; set; }
        public string San { get; set; }
        public string KeepExisting { get; set; }
        public string Script { get; set; }
        public string ScriptParameters { get; set; }
        public bool Warmup { get; set; }

        public override string ToString() => $"{Binding}: {R.Renewafter} {Date.ToShortDateString()}";

        internal string Save()
        {
            return JsonConvert.SerializeObject(this);
        }

        internal static ScheduledRenewal Load(string json)
        {
            return JsonConvert.DeserializeObject<ScheduledRenewal>(json);
        }
    }
}