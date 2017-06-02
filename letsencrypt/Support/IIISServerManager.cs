using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;

namespace letsencrypt.Support
{
    public interface IIISServerManager : IDisposable
    {
        void CommitChanges();
        Version GetVersion();
        IEnumerable<Site> Sites { get; }
    }
}
