resource "azurerm_key_vault" "kv_complex_cases" {
  name                = "${local.product_name_prefix}-kv"
  location            = azurerm_resource_group.rg_complex_cases.location
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  tenant_id           = data.azurerm_client_config.current.tenant_id

  enable_rbac_authorization       = true
  enabled_for_template_deployment = true
  public_network_access_enabled   = false

  sku_name = "standard"

  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
    virtual_network_subnet_ids = [
    ]
  }

  tags = local.common_tags
}

resource "azurerm_private_endpoint" "kv_complex_cases_pe" {
  name                = "${azurerm_key_vault.kv_complex_cases.name}-pe"
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  location            = azurerm_resource_group.rg_complex_cases.location
  subnet_id           = data.azurerm_subnet.complex_cases_placeholder_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_keyvault.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_keyvault.id]
  }

  private_service_connection {
    name                           = "${azurerm_key_vault.kv_complex_cases.name}-psc"
    private_connection_resource_id = azurerm_key_vault.kv_complex_cases.id
    is_manual_connection           = false
    subresource_names              = ["vault"]
  }
}

#begin: assign roles

resource "azurerm_role_assignment" "kv_role_terraform_sp" {
  scope                = azurerm_key_vault.kv_complex_cases.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azuread_service_principal.terraform_service_principal.object_id
}

resource "azurerm_role_assignment" "kv_role_complex_cases_ui_crypto_user" {
  scope                = azurerm_key_vault.kv_complex_cases.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = azurerm_linux_web_app.complex_cases_ui.identity[0].principal_id

  depends_on = [azurerm_linux_web_app.complex_cases_ui]
}

resource "azurerm_role_assignment" "kv_role_complex_cases_ui_secrets_user" {
  scope                = azurerm_key_vault.kv_complex_cases.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_web_app.kv_complex_cases.identity[0].principal_id

  depends_on = [azurerm_linux_web_app.kv_complex_cases]
}

#end: assign roles

#begin: store values

resource "azurerm_key_vault_secret" "kvs_complex_cases_ui_client_secret" {
  name         = "ComplexCasesUIClientSecret"
  value        = azuread_application_password.pwd_complex_cases_ui.value
  key_vault_id = azurerm_key_vault.kv_complex_cases.id
  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp,
    azuread_application_password.complex_cases_ui
  ]
}

resource "azurerm_key_vault_secret" "kvs_complex_cases_api_client_secret" {
  name         = "ComplexCasesAPIClientSecret"
  value        = azuread_application_password.pwd_complex_cases_api.value
  key_vault_id = azurerm_key_vault.kv_complex_cases.id
  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp,
    azuread_application_password.complex_cases_api
  ]
}

resource "azurerm_key_vault_secret" "kvs_complex_cases_proxy_client_secret" {
  name         = "ComplexCasesProxyClientSecret"
  value        = azuread_application_password.pwd_complex_cases_cms_proxy.value
  key_vault_id = azurerm_key_vault.kv_complex_cases.id
  depends_on = [
    azurerm_role_assignment.kv_role_terraform_sp,
    azuread_application_password.com
  ]
}

#end: store values