resource "azuread_application" "sharepoint_embedded" {
  display_name            = "${local.product_name}${local.shared_prefix}-sharepoint-embedded-appreg"
  prevent_duplicate_names = true

  api {
    requested_access_token_version = 1
    oauth2_permission_scope {
      admin_consent_description  = "The application can call this app's API to manage SharePoint Embedded Storage Containers."
      admin_consent_display_name = "Manage Sharepoint Embedded Containers"
      id                         = element(random_uuid.random_id[*].result, 0)
      enabled                    = true
      type                       = "Admin"
      user_consent_description   = "The application can call this app's API to manage SharePoint Embedded Storage Containers."
      user_consent_display_name  = "Manage Sharepoint Embedded Containers"
      value                      = "Container.Manager"
    }
  }

  required_resource_access {
    resource_app_id = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["Files.ReadWrite.All"]
      type = "Scope"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.app_role_ids["Files.ReadWrite.All"]
      type = "Role"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["FileStorageContainer.Selected"]
      type = "Scope"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.app_role_ids["FileStorageContainer.Selected"]
      type = "Role"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["Sites.Read.All"]
      type = "Scope"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["User.Read"]
      type = "Scope"
    }
  }

  required_resource_access {
    resource_app_id = data.azuread_application_published_app_ids.well_known.result["Office365SharePointOnline"]

    resource_access {
      id   = data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["AllSites.Read"]
      type = "Scope"
    }

    resource_access {
      id   = data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["AllSites.Write"]
      type = "Scope"
    }

    resource_access {
      id   = data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["Sites.ReadWrite.All"]
      type = "Role"
    }
  }

  required_resource_access {
    resource_app_id = azuread_application.complex_cases_api.client_id

    dynamic "resource_access" {
      for_each = azuread_application.complex_cases_api.api.0.oauth2_permission_scope
      iterator = scope

      content {
        id   = scope.value.id
        type = "Scope"
      }
    }
  }

  single_page_application {
    redirect_uris = ["http://localhost"]
  }

  web {
    redirect_uris = ["http://localhost:7071", "https://oauth.pstmn.io/v1/browser-callback", "https://oauth.pstmn.io/v1/callback"]

    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = false
    }
  }
}