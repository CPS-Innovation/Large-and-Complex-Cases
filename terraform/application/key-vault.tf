resource "azurerm_key_vault" "kv_complex_cases" {
  name                = "${local.product_prefix}-kv"
  location            = azurerm_resource_group.rg_complex_cases.location
  resource_group_name = azurerm_resource_group.rg_complex_cases.name
  tenant_id           = data.azurerm_client_config.current.tenant_id

  enable_rbac_authorization       = true
  enabled_for_template_deployment = true
  public_network_access_enabled   = true
  purge_protection_enabled        = true
  soft_delete_retention_days      = 7 # default is 90

  sku_name = "standard"

  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
    virtual_network_subnet_ids = [
      data.azurerm_subnet.build_agent_subnet.id,
      azurerm_subnet.sn_complex_cases_api_subnet.id,
      azurerm_subnet.sn_complex_cases_endpoints_subnet.id,
      azurerm_subnet.sn_complex_cases_ui_subnet.id
    ]
  }

  tags = local.common_tags
}

#resource "azurerm_private_endpoint" "kv_complex_cases_pe" {
#  name                = "${azurerm_key_vault.kv_complex_cases.name}-pe"
#  resource_group_name = azurerm_resource_group.rg_complex_cases.name
#  location            = azurerm_resource_group.rg_complex_cases.location
#  subnet_id           = azurerm_subnet.sn_complex_cases_endpoints_subnet.id
#  tags                = local.common_tags
#
#  private_dns_zone_group {
#    name                 = data.azurerm_private_dns_zone.dns_zone_keyvault.name
#    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_keyvault.id]
#  }

#  private_service_connection {
#    name                           = "${azurerm_key_vault.kv_complex_cases.name}-psc"
#    private_connection_resource_id = azurerm_key_vault.kv_complex_cases.id
#    is_manual_connection           = false
#    subresource_names              = ["vault"]
#  }
#}

#begin: assign roles

resource "azurerm_role_assignment" "kv_role_terraform_sp" {
  scope                = azurerm_key_vault.kv_complex_cases.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azuread_service_principal.terraform_service_principal.object_id
}

resource "azurerm_role_assignment" "kv_role_complex_cases_ui_secrets_user" {
  scope                = azurerm_key_vault.kv_complex_cases.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = azurerm_linux_web_app.complex_cases_ui.identity[0].principal_id

  depends_on = [azurerm_linux_web_app.complex_cases_ui]
}

resource "azurerm_role_assignment" "kv_role_complex_cases_api_secrets_user" {
  scope                = azurerm_key_vault.kv_complex_cases.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = azurerm_linux_function_app.complex_cases_api.identity[0].principal_id

  depends_on = [azurerm_linux_function_app.complex_cases_api]
}

#end: assign roles
