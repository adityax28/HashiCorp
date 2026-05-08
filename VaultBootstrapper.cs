using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.Token;

namespace API.Vault;

public static class VaultBootstrapper
{
    public static void TryAddVaultConnectionString(IConfigurationBuilder config, IHostEnvironment env)
    {
        var enabled = GetEnvBool("VAULT_ENABLED");
        if (enabled is false)
        {
            return;
        }

        var vaultAddr = GetEnv("VAULT_ADDR");
        if (string.IsNullOrWhiteSpace(vaultAddr))
        {
            return;
        }

        var kvMount = GetEnv("VAULT_KV_MOUNT") ?? "kv";
        var kvPath = GetEnv("VAULT_KV_PATH") ?? "app/database/ConnectionStrings";
        var kvField = GetEnv("VAULT_KV_FIELD") ?? "DefaultDatabase";


        // IMP - $env:VAULT_REQUIRED="true"
        // If this is true, then application only starts when Vault server is start 
        var required = GetEnvBool("VAULT_REQUIRED") ?? !env.IsDevelopment();

        try
        {
            var connectionString = ReadKvV2String(vaultAddr: vaultAddr, kvMount: kvMount, kvPath: kvPath, kvField: kvField);


            // Vault returned empty value, treat as not found.
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Vault KV value is empty for mount='{kvMount}', path='{kvPath}', field='{kvField}'."
                );
            }

            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultDatabase"] = connectionString,
                }
            );
        }
        catch when (!required)
        {
            // Optional in local/dev unless explicitly required.
        }
    }

    private static string ReadKvV2String(string vaultAddr, string kvMount, string kvPath, string kvField)
    {
        var client = CreateVaultClient(vaultAddr);
        var secret = client.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: kvPath, mountPoint: kvMount)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        if (secret?.Data?.Data is null || secret.Data.Data.Count == 0)
        {
            throw new InvalidOperationException($"Vault returned no data for mount='{kvMount}', path='{kvPath}'.");
        }

        if (!secret.Data.Data.TryGetValue(kvField, out var raw) || raw is null)
        {
            throw new KeyNotFoundException(
                $"Vault KV field '{kvField}' not found at mount='{kvMount}', path='{kvPath}'."
            );
        }

        return raw.ToString() ?? string.Empty;
    }

    private static IVaultClient CreateVaultClient(string vaultAddr)
    {
        IAuthMethodInfo authMethod = CreateAuthMethod();
        var settings = new VaultClientSettings(vaultAddr, authMethod)
        {
            ContinueAsyncTasksOnCapturedContext = false,
        };

        return new VaultClient(settings);
    }

    private static IAuthMethodInfo CreateAuthMethod()
    {
        var token = GetEnv("VAULT_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
        {
            return new TokenAuthMethodInfo(token);
        }

        var roleId = GetEnv("VAULT_ROLE_ID");
        var secretId = GetEnv("VAULT_SECRET_ID");
        if (!string.IsNullOrWhiteSpace(roleId) && !string.IsNullOrWhiteSpace(secretId))
        {
            return new AppRoleAuthMethodInfo(roleId, secretId);
        }

        throw new InvalidOperationException(
            "Vault auth not configured. Provide VAULT_TOKEN or (VAULT_ROLE_ID and VAULT_SECRET_ID)."
        );
    }

    private static string? GetEnv(string name) => Environment.GetEnvironmentVariable(name);

    private static bool? GetEnvBool(string name)
    {
        var value = GetEnv(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase)
            || value.Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
    }
}

