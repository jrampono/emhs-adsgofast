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
        private const string Reader = "Reader";
        private const string ReaderGroupID = "ReaderSecurityId";

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
            options.Value.SecurityRoles[Reader].AllowActions.Count.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void CanPerformActionAllowedInGlobalList()
        {
            var options = GetSecurityAccessProviderOptions();
            var sec = new SecurityAccessProvider(options);
            var id = GetIdentity();

            var result = sec.CanPerformOperation("ControllerA", "List", id);

            options.Value.SecurityRoles[Reader].AllowActions.IndexOf("ControllerA.List").Should().Be(-1);
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

            options.Value.SecurityRoles[Reader].AllowActions.IndexOf("ControllerA.Delete").Should().BeGreaterOrEqualTo(0);
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

            options.Value.SecurityRoles[Reader].AllowActions.IndexOf("ControllerA.DoAThing").Should().Be(-1);
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

            options.Value.SecurityRoles[Reader].AllowActions.IndexOf("ControllerB.Details").Should().BeGreaterOrEqualTo(0);
            options.Value.GlobalAllowActions.IndexOf("ControllerB.Details").Should().BeGreaterOrEqualTo(0);
            options.Value.GlobalDenyActions.IndexOf("ControllerB").Should().BeGreaterOrEqualTo(0);
            result.Should().Be(false);
        }


        [TestMethod]
        public void GrantActionBasedOnAliasedActions()
        {

            var options = GetSecurityAccessProviderOptions();
            var sec = new SecurityAccessProvider(options);
            var id = GetIdentity("AliasAppliedRoleId");

            sec.CanPerformOperation("GlobalAppliedController", "ReaderAppliedAction1", id).Should().Be(true);
            sec.CanPerformOperation("GlobalAppliedController", "ReaderAppliedAction2", id).Should().Be(true);
            sec.CanPerformOperation("GlobalAppliedController", "WriterAppliedAction1", id).Should().Be(true);
            sec.CanPerformOperation("GlobalAppliedController", "WriterAppliedAction2", id).Should().Be(true);
            sec.CanPerformOperation("UserAppliedController", "ReaderAppliedAction1", id).Should().Be(true);
            sec.CanPerformOperation("UserAppliedController", "ReaderAppliedAction2", id).Should().Be(true);
            sec.CanPerformOperation("UserAppliedController", "WriterAppliedAction1", id).Should().Be(true);
            sec.CanPerformOperation("UserAppliedController", "WriterAppliedAction2", id).Should().Be(true);
            sec.CanPerformOperation("UserAppliedController", "UnknownAction", id).Should().Be(false);
            sec.CanPerformOperation("OtherController", "ReaderAppliedAction1", id).Should().Be(false);
            sec.CanPerformOperation("OtherController", "ReaderAppliedAction2", id).Should().Be(false);
        }


        [TestMethod]
        public void GrantActionBasedOnWildcardControllerAliasedAction()
        {

            var options = GetSecurityAccessProviderOptions();
            var sec = new SecurityAccessProvider(options);
            var id = GetIdentity("WildcardAppliedRoleId");

            sec.CanPerformOperation("UndefinedController", "ReaderAppliedAction1", id).Should().Be(true);
            sec.CanPerformOperation("UndefinedController", "ReaderAppliedAction2", id).Should().Be(true);
            sec.CanPerformOperation("UndefinedController", "WriterAppliedAction1", id).Should().Be(true);
            sec.CanPerformOperation("UndefinedController", "WriterAppliedAction2", id).Should().Be(true);
            sec.CanPerformOperation("UndefinedController", "Create", id).Should().Be(true);
            sec.CanPerformOperation("UndefinedController", "UnknownAction", id).Should().Be(false);
        }


        [TestMethod]
        public void GrantActionBasedOnInnerWildCardActionName()
        {

            var options = GetSecurityAccessProviderOptions();
            var sec = new SecurityAccessProvider(options);
            var id = GetIdentity();

            //test the wildcard inclusion
            sec.CanPerformOperation("ControllerA", "WildCardItem", id).Should().Be(true);
            sec.CanPerformOperation("ControllerA", "WildCardItem*", id).Should().Be(true);
            sec.CanPerformOperation("ControllerA", "WildCardItemRANDOMTEXT", id).Should().Be(true);

            //varify that it's not caught up in a full-action wildcard assignmenent
            sec.CanPerformOperation("ControllerA", "BoopWildCardItemRANDOMTEXT", id).Should().Be(false);
            sec.CanPerformOperation("ControllerB", "WildCardItem*", id).Should().Be(false);
            sec.CanPerformOperation("ControllerB", "WildCardItemRANDOMTEXT", id).Should().Be(false);
        }


        [TestMethod]
        public void ListApplicableRolesForAction()
        {

            var options = GetSecurityAccessProviderOptions();
            var sec = new SecurityAccessProvider(options);

            sec.GetRolesForAction("UndefinedController", "ReaderAppliedAction1").Should().BeEquivalentTo(new []{ "WildcardAppliedRole"});
        }


        private ClaimsIdentity GetIdentity() => GetIdentity(ReaderGroupID);
        private ClaimsIdentity GetIdentity(params string[] groups)
        {
            var id = new ClaimsIdentity("pwd");

            foreach(var g in groups)
            {
                id.AddClaim(new Claim("groups", g));
            }

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
