using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplication.Models.Options;
using WebApplication.Services;
using System.Security.Claims;

namespace WebApplication.Tests
{
    [TestClass]
    public class SecurityAccessProviderTests
    {
        [TestMethod]
        public void LoadingSettingsIncludesGlobalListsAndRoles()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appSettings.json");
            var config = configBuilder.Build();
            var optionsModel = ConfigurationBinder.Get<SecurityModelOptions>(config.GetSection("SecurityModelOptions"));
            var options = Options.Create(optionsModel);

            options.Should().NotBeNull();
            options.Value.GlobalAllowActions.Count.Should().BeGreaterThan(0);
            options.Value.GlobalDenyActions.Count.Should().BeGreaterThan(0);
            options.Value.SecurityRoles.Count.Should().BeGreaterThan(0);
            options.Value.SecurityRoles[0].AllowActions.Count.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void CanPerformActionAllowedInGlobalList()
        {
            var options = GetSecurityAccessProviderOptions();
            var sec = new SecurityAccessProvider(options);
            var id = GetIdentity();

            var result = sec.CanPerformOperation("ControllerA", "List", id);

            options.Value.SecurityRoles[0].AllowActions.IndexOf("ControllerA.List").Should().Be(-1);
            options.Value.GlobalAllowActions.IndexOf("ControllerA.List").Should().BeGreaterOrEqualTo(0);
            options.Value.GlobalDenyActions.IndexOf("ControllerA.List").Should().Be(-1);
            options.Value.GlobalDenyActions.IndexOf("ControllerA").Should().Be(-1);
            result.Should().Be(true);
        }

        [TestMethod]
        public void CantPerformActionDeniedInInGlobalListEvenIfGrantedToRole()
        {
            var options = GetSecurityAccessProviderOptions();
            var sec = new SecurityAccessProvider(options);
            var id = GetIdentity();

            var result = sec.CanPerformOperation("ControllerA", "Delete", id);

            options.Value.SecurityRoles[0].AllowActions.IndexOf("ControllerA.Delete").Should().BeGreaterOrEqualTo(0);
            options.Value.GlobalAllowActions.IndexOf("ControllerA.Delete").Should().Be(-1);
            options.Value.GlobalDenyActions.IndexOf("ControllerA.Delete").Should().BeGreaterOrEqualTo(0);
            options.Value.GlobalDenyActions.IndexOf("ControllerA").Should().Be(-1);
            result.Should().Be(false);
        }

        [TestMethod]
        public void CantPerformUnconfiguredROles()
        {
            var options = GetSecurityAccessProviderOptions();
            var sec = new SecurityAccessProvider(options);
            var id = GetIdentity();

            var result = sec.CanPerformOperation("ControllerA", "DoAThing", id);

            options.Value.SecurityRoles[0].AllowActions.IndexOf("ControllerA.DoAThing").Should().Be(-1);
            options.Value.GlobalAllowActions.IndexOf("ControllerA.DoAThing").Should().Be(-1);
            options.Value.GlobalDenyActions.IndexOf("ControllerA.DoAThing").Should().Be(-1);
            options.Value.GlobalDenyActions.IndexOf("ControllerA").Should().Be(-1);
            result.Should().Be(false);
        }


        [TestMethod]
        public void CantPerformActionNotGranted()
        {
            var options = GetSecurityAccessProviderOptions();
            var sec = new SecurityAccessProvider(options);
            var id = GetIdentity();

            var result = sec.CanPerformOperation("ControllerA", "Details", id);

            result.Should().Be(false);
        }

        [TestMethod]
        public void GlobalDeniedControllersShouldTakePrecenceOverExplicitGrants()
        {
            var options = GetSecurityAccessProviderOptions();
            var sec = new SecurityAccessProvider(options);
            var id = GetIdentity();

            var result = sec.CanPerformOperation("ControllerB", "Details", id);

            options.Value.SecurityRoles[0].AllowActions.IndexOf("ControllerB.Details").Should().BeGreaterOrEqualTo(0);
            options.Value.GlobalAllowActions.IndexOf("ControllerB.Details").Should().BeGreaterOrEqualTo(0);
            options.Value.GlobalDenyActions.IndexOf("ControllerB").Should().BeGreaterOrEqualTo(0);
            result.Should().Be(false);
        }



        private ClaimsIdentity GetIdentity()
        {
            var id = new ClaimsIdentity("pwd");
            id.AddClaim(new Claim("groups", "ReaderSecurityId"));

            return id;
            
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
    }
}
