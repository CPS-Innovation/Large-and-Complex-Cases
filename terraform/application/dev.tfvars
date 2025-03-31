environment = {
  name  = "development"
  alias = "dev"
}

terraform_service_principal_display_name = "Azure Pipeline: LaCC-PreProd"
dns_server                               = "10.7.197.20"
dns_alt_server                           = "168.63.129.16"
subscription_id                          = "7f67e716-03c5-4675-bad2-cc5e28652759"
nsg_name                                 = "basicNsgVNET-LaCC-WANNET-nic01"

service_plans = {
  ui_service_plan_sku   = "P1v3"
  ui_worker_count       = 2
  api_service_plan_sku  = "P1v3"
  api_worker_count      = 2
  mock_service_plan_sku = "P0v3"
  mock_worker_count     = 2
}

service_capacity = {
  ui_default_capacity   = 1
  ui_minimum_capacity   = 1
  ui_max_capacity       = 3
  api_default_capacity  = 1
  api_minimum_capacity  = 1
  api_max_capacity      = 1
  mock_default_capacity = 1
  mock_minimum_capacity = 1
  mock_max_capacity     = 2
}

subnets = {
  storage   = ["10.7.184.64/28"]
  ui        = ["10.7.184.96/27"]
  api       = ["10.7.184.128/27"]
  endpoints = ["10.7.184.160/27"]
  mock      = ["10.7.184.80/28"]
}