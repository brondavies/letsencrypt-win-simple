using System;
using letsencrypt.Support;
using Microsoft.Web.Administration;
using Microsoft.Web.Administration.Fakes;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace letsencrypt_tests.Support
{
    internal class MockIISServerManager : IIISServerManager
    {
        public IEnumerable<Site> Sites
        {
            get
            {
                return new Site[] { new ShimSite {
                    IdGet = () => 0,
                    BindingsGet = () => FakeBindings,
                    ApplicationsGet = () => FakeApplications //Applications["/"].VirtualDirectories["/"].PhysicalPath
                } };
            }
        }

        private BindingCollection FakeBindings
        {
            get
            {
                var col = new ShimBindingCollection();
                var bindings = new Binding[] { new ShimBinding {
                        HostGet = () => "localhost",
                        ProtocolGet = () => "http"
                    } };
                col.Bind((IEnumerable<Binding>)bindings);

                return col;
            }
        }

        private ApplicationCollection FakeApplications
        {
            get
            {
                var col = new ShimApplicationCollection();
                col.ItemGetString = (s) =>
                {
                    return new ShimApplication
                    {
                        VirtualDirectoriesGet = () =>
                        {
                            return FakeVirtualDirectories;
                        }
                    };
                };

                return col;
            }
        }

        private VirtualDirectoryCollection FakeVirtualDirectories
        {
            get
            {
                var col = new ShimVirtualDirectoryCollection();
                col.ItemGetString = (s) =>
                {
                    return new ShimVirtualDirectory
                    {
                        PhysicalPathGet = () => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    };
                };

                return col;
            }
        }

        public void CommitChanges()
        {
        }

        public void Dispose()
        {
        }

        public Version GetVersion()
        {
            return new Version(8, 0);
        }
    }
}