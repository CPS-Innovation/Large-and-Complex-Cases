resource "azurerm_linux_web_app" "complex_cases_egressMock" {
  name                          = "${local.product_prefix}-egress-mock"
  resource_group_name           = azurerm_resource_group.rg_complex_cases.name
  location                      = azurerm_resource_group.rg_complex_cases.location
  service_plan_id               = azurerm_service_plan.asp_complex_cases_egressMock.id
  virtual_network_subnet_id     = data.azurerm_subnet.complex_cases_egressMock_subnet.id
  public_network_access_enabled = false
  https_only                    = true

  app_settings = {
    "FirstTimeDeployment"                        = "1"
    "WEBSITE_CONTENTOVERVNET"                    = "1"
    "WEBSITE_DNS_SERVER"                         = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                     = var.dns_alt_server
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"   = azurerm_storage_account.sacpsccui.primary_connection_string
    "WEBSITE_CONTENTSHARE"                       = azapi_resource.sacpsccui_egressMock_file_share.name
    "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"
    "XDT_MicrosoftApplicationInsights_Mode"      = "Recommended"
  }
  site_config {
    ftps_state                              = "FtpsOnly"
    http2_enabled                           = true
    always_on                               = true
    vnet_route_all_enabled                  = true
    container_registry_use_managed_identity = false
    application_stack {
      dotnet_version = "8.0"
    }
  }

  auth_settings_v2 {
    auth_enabled           = false
    unauthenticated_action = "AllowAnonymous"
    default_provider       = "AzureActiveDirectory"

    active_directory_v2 {
      tenant_auth_endpoint       = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/v2.0"
      client_secret_setting_name = "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
      client_id                  = azuread_application.complex_cases_egressMock.client_id
    }

    login {
      token_store_enabled = false
    }
  }

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      app_settings["FirstTimeDeployment"]
    ]
  }
}

resource "azuread_application" "complex_cases_egressMock" {
  display_name            = "${local.product_prefix}-egressMock-appreg"
  identifier_uris         = ["https://CPSGOVUK.onmicrosoft.com/${local.product_prefix}-egressMock"]
  prevent_duplicate_names = true
  owners                  = [data.azuread_service_principal.terraform_service_principal.object_id]

  required_resource_access {
    resource_app_id = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["User.Read"]
      type = "Scope"
    }
  }
}