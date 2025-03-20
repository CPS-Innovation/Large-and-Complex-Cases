environment = {
  name  = "pre-production"
  alias = "preprod"
}

terraform_service_principal_display_name = "Azure Pipeline: LaCC-PreProd"

appinsights_configuration = {
  log_retention_days                   = 90
  log_total_retention_days             = 2555
  analytics_internet_ingestion_enabled = false
  analytics_internet_query_enabled     = false
  insights_internet_ingestion_enabled  = true
  insights_internet_query_enabled      = false
}


subnets = {
  ampls      = ["10.7.184.32/28"]
}