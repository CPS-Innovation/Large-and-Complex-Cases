environment = {
  name  = "development"
  alias = "dev"
}

terraform_service_principal_display_name = "Azure Pipeline: Complex-Cases-Development"

appinsights_configuration = {
  log_retention_days                   = 90
  log_total_retention_days             = 2555
  analytics_internet_ingestion_enabled = false
  analytics_internet_query_enabled     = false
  insights_internet_ingestion_enabled  = true
  insights_internet_query_enabled      = false
}


subnets {
  ampls      = ["10.7.184.32/28"]
  storage    = ["10.7.184.48/28"]
  ui         = ["10.7.184.64/27"]
  api        = ["10.7.184.96/27"]
  endpoints  = ["10.7.184.128/27"]
  egressMock = ["10.7.184.160/28"]
  netAppMock = ["10.7.184.176/28"]
}