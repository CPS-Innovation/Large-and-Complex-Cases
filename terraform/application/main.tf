terraform {
  required_version = ">= 1.5.3"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.18.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "3.1.0"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.6.3"
    }

    azapi = {
      source  = "Azure/azapi"
      version = "2.2.0"
    }
  }

  backend "azurerm" {
  }
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
    key_vault {
      purge_soft_delete_on_destroy          = true
      purge_soft_deleted_keys_on_destroy    = true
      purge_soft_deleted_secrets_on_destroy = true
      recover_soft_deleted_key_vaults       = true
      recover_soft_deleted_keys             = true
      recover_soft_deleted_secrets          = true
    }
  }
}

data "azurerm_client_config" "current" {}

data "azuread_service_principal" "terraform_service_principal" {
  display_name = var.terraform_service_principal_display_name
}

data "azurerm_subscription" "current" {}

data "azuread_application_published_app_ids" "well_known" {}

resource "azuread_service_principal" "msgraph" {
  client_id    = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]
  use_existing = true
}

resource "azuread_service_principal" "sharepointonline" {
  client_id    = data.azuread_application_published_app_ids.well_known.result["Office365SharePointOnline"]
  use_existing = true
}

resource "random_uuid" "random_id" {
  count = 2
}

locals {
  product_name       = "lacc"
  resource_suffix    = var.environment.alias != "prod" ? "-${var.environment.alias}" : ""
  shared_suffix      = var.environment.alias != "prod" ? "preprod" : ""
  shared_prefix      = var.environment.alias != "prod" ? "-preprod" : ""
  product_prefix     = "${local.product_name}${local.resource_suffix}"
  ddei_resource_name = "${local.product_name}${local.resource_suffix}-ddei"
  common_tags = {
    environment = var.environment.name
    project     = ""
    creator     = "Created by Terraform"
  }
}
