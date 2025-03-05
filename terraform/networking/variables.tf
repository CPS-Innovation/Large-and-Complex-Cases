variable "environment" {
  type = object({
    name  = string
    alias = string
  })
}

variable "location" {
  default = "UK South"
}

variable "terraform_service_principal_display_name" {
  type = string
}

variable "appinsights_configuration" {
  type = object({
    log_retention_days                   = number
    log_total_retention_days             = number
    analytics_internet_ingestion_enabled = bool
    analytics_internet_query_enabled     = bool
    insights_internet_ingestion_enabled  = bool
    insights_internet_query_enabled      = bool
  })
}

variable "subnets" {
  type = object({
    ampls      = list(string)
    storage    = list(string)
    ui         = list(string)
    api        = list(string)
    endpoints  = list(string)
    egressMock = list(string)
    netAppMock = list(string)
  })
}