using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using IAMRoleService.WebApi.Models;

namespace IAMRoleService.WebApi.Infrastructure.Aws
{
    public class RoleFactory : IRoleFactory
    {
        private readonly IAmazonIdentityManagementService _client;
        private readonly IAmazonSecurityTokenService _securityTokenServiceClient;

        public RoleFactory(IAmazonIdentityManagementService client, IAmazonSecurityTokenService securityTokenServiceClient)
        {
            _client = client;
            _securityTokenServiceClient = securityTokenServiceClient;
        }

        
        public async Task<Role> CreateStsAssumableRoleAsync(string roleName)
        {
            var identityResponse = await _securityTokenServiceClient.GetCallerIdentityAsync(new GetCallerIdentityRequest());
            var accountArn = new AwsAccountArn(identityResponse.Account);

            var request = CreateStsAssumableRoleRequest(accountArn, roleName);
            var response = await _client.CreateRoleAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                var metadata = string.Join(", ", response.ResponseMetadata.Metadata.Select(m => $"{m.Key}:{m.Value}"));
                throw new Exception($"Error creating role: \"{roleName}\". Status code was {response.HttpStatusCode}, metadata: {metadata}");
            }
            
            return response.Role;
        }

        
        public CreateRoleRequest CreateStsAssumableRoleRequest(AwsAccountArn accountArn, string roleName)
        {
            return new CreateRoleRequest
            {
                
                
                RoleName = roleName,
                AssumeRolePolicyDocument = 
                    @"{""Version"":""2012-10-17"",""Statement"":[{""Effect"":""Allow"",""Principal"":{""Federated"":""" + 
                    accountArn + ":saml-provider/ADFS" + 
                    @"""},""Action"":""sts:AssumeRoleWithSAML"", ""Condition"": {""StringEquals"": {""SAML:aud"": ""https://signin.aws.amazon.com/saml""}}}]}"
            }; 
        }
    }
}