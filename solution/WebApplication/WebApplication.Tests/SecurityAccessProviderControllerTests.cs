using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplication.Controllers;
using WebApplication.Models.Options;
using WebApplication.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebApplication.Models;

namespace WebApplication.Tests
{
    [TestClass]
    public class SecurityAccessProviderControllerTests
    {
        [TestMethod]
        public async Task CanPerformCurrentActionOnRecord_NullRecord_ReturnsGlobalRole()
        {
            var user = GetIdentity("WildcardAppliedRoleId");
            var rp = new FakeRoleProvider();
            var controller = new TestController(new SecurityAccessProvider(GetSecurityAccessProviderOptions()), rp, user);

            rp.Roles = new[] { "WildcardAppliedRole" };

            (await controller.ExecCanPerformCurrentActionOnRecord(null, "BogusAction")).Should().BeFalse();
            (await controller.ExecCanPerformCurrentActionOnRecord(null, "ReaderAppliedAction1")).Should().BeTrue();
        }

        [TestMethod]
        public async Task CanPerformCurrentActionOnRecord_NoRoleFromEntity_ReturnsGlobalRole()
        {
            var user = GetIdentity("WildcardAppliedRoleId");
            var rp = new FakeRoleProvider();
            var controller = new TestController(new SecurityAccessProvider(GetSecurityAccessProviderOptions()), rp, user);

            (await controller.ExecCanPerformCurrentActionOnRecord(new object(), "BogusAction")).Should().BeFalse();
            (await controller.ExecCanPerformCurrentActionOnRecord(new object(), "ReaderAppliedAction1")).Should().BeTrue();
        }

        [TestMethod]
        public async Task CanPerformCurrentActionOnRecord_RoleFromEntity_ReturnsProvidedRole()
        {
            var user = GetIdentity();
            var rp = new FakeRoleProvider();
            var controller = new TestController(new SecurityAccessProvider(GetSecurityAccessProviderOptions()), rp, user);

            rp.Roles = new[] { "WildcardAppliedRole" };

            (await controller.ExecCanPerformCurrentActionOnRecord(new object(), "BogusAction")).Should().BeFalse();
            (await controller.ExecCanPerformCurrentActionOnRecord(new object(), "ReaderAppliedAction1")).Should().BeTrue();
        }

        private class TestController : BaseController
        {
            public TestController(ISecurityAccessProvider sap, IEntityRoleProvider roleProvider, ClaimsIdentity user) : base(sap, roleProvider)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(user)
                    }
                };
            }
            public async Task<bool> ExecCanPerformCurrentActionOnRecord(object record, string action) => await base.CanPerformCurrentActionOnRecord(record, action);
        }

        private class FakeRoleProvider : IEntityRoleProvider
        {
            public string[] Roles { get; set; } = new string[0];

            public Task<string[]> GetUserRoles(object entity, Guid[] groups)
            {
                return Task.FromResult(Roles);
            }
        }

        private IOptions<SecurityModelOptions> GetSecurityAccessProviderOptions()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appSettings.json");
            var config = configBuilder.Build();
            var optionsModel = ConfigurationBinder.Get<SecurityModelOptions>(config.GetSection("SecurityModelOptions"));
            var options = Options.Create(optionsModel);
            return options;
        }

        private ClaimsIdentity GetIdentity(params string[] groups)
        {
            var id = new ClaimsIdentity("pwd");

            foreach (var g in groups)
            {
                id.AddClaim(new Claim("groups", g));
            }

            return id;
        }

    }
}
