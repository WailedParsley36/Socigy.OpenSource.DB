using Socigy.OpenSource.DB.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.DB.Tests
{
    public abstract class BaseUnitTest
    {
        protected IDbConnectionFactory ConnectionFactory => UnitCore.ConnectionFactory;

        [OneTimeSetUp]
        public async void Initialize()
        {
            await UnitCore.InitializeHostAsync();
        }
    }
}
