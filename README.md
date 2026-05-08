# HashiCorp
HashiCorp Setup for Secure database connection string
-----------------------------------------------------------------------------
# HashiCorp Vault Setup Guide (Windows)

## 1. Download Vault
Download HashiCorp Vault from:

[HashiCorp Vault Downloads](https://developer.hashicorp.com/vault/downloads?utm_source=chatgpt.com)

---

## 2. Move Vault
Move `vault.exe` to any folder or keep it inside the `Downloads` folder.

---

## 3. Create `vault.hcl`
Create a file named `vault.hcl` inside the `Downloads` folder.

### `vault.hcl`

```hcl
storage "file" {
  path = "C:/vault/data"
}

listener "tcp" {
  address = "127.0.0.1:8200"
  tls_disable = 1
}

ui = true
```

---

# Start Vault Server

## 4. Open Terminal
Go to the Downloads directory.

### PowerShell / CMD

```powershell
cd Downloads
```

---

## 5. Start Vault Server

### PowerShell

```powershell
.\vault.exe server -config="vault.hcl"
```

### CMD

```cmd
vault.exe server -config="vault.hcl"
```

---

# Configure Vault Environment

## 6. Open New Terminal
Open another terminal window and move to Downloads folder.

```powershell
cd $env:USERPROFILE\Downloads
```

---

## 7. Set Vault Address

```powershell
$env:VAULT_ADDR="http://127.0.0.1:8200"
```

---

## 8. Check Vault Status

```powershell
.\vault.exe status
```

Expected Output:

```text
Sealed = true
```

---

# Initialize Vault

## 9. Initialize Vault

```powershell
.\vault.exe operator init
```

⚠️ Important:
- Copy all output
- Save all unseal keys
- Save root token securely

---

# Unseal Vault

## 10. Unseal Vault (Run 3 Times)

```powershell
.\vault.exe operator unseal
```

Use a different unseal key each time.

Repeat this process 3 times.

---

## 11. Verify Vault Status

```powershell
.\vault.exe status
```

Expected Output:

```text
Sealed: false
```

✅ That means Vault is unlocked.

---

# Login to Vault

## 12. Login

```powershell
.\vault.exe login
```

Paste the root token generated during initialization.

---

# Add Secrets

## 13. Add KV Key/Values
Use the Vault UI to add KV secrets.

Vault UI:

```text
http://127.0.0.1:8200
```

---

# Application Environment Variables

## 14. Setup Environment Variables

### Minimum Configuration

```powershell
$env:VAULT_ENABLED="true"
$env:VAULT_ADDR="http://127.0.0.1:8200"
$env:VAULT_KV_MOUNT="kv"
```

---

### Full Configuration

```powershell
$env:VAULT_ENABLED="true"
$env:VAULT_ADDR="http://127.0.0.1:8200"
$env:VAULT_KV_MOUNT="kv"
$env:VAULT_KV_PATH="app/database/ConnectionStrings"
$env:VAULT_KV_FIELD="DefaultDatabase"
$env:VAULT_REQUIRED="true"
```

---

## Important

```powershell
$env:VAULT_REQUIRED="true"
```

If this is set to `true`, then the application will only start when the Vault server is running.

---

# Next Time Startup Process

From next time onwards:

## 1. Start Vault Server

```powershell
.\vault.exe server -config="vault.hcl"
```

---

## 2. Unseal Vault (3 Times)

```powershell
.\vault.exe operator unseal
```

Use different unseal keys each time.

---

## 3. Login

```powershell
.\vault.exe login
```

---
