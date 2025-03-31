environment = {
  name  = "production"
  alias = "prod"
}

terraform_service_principal_display_name = "Azure Pipeline: LaCC-Prod"
dns_server                               = "10.7.204.164"
dns_alt_server                           = "168.63.129.16"
subscription_id                          = "[Placeholder]"
nsg_name                                 = "[Placeholder]"

service_plans = {
  ui_service_plan_sku   = "P1mv3"
  ui_worker_count       = 2
  api_service_plan_sku  = "P1mv3"
  api_worker_count      = 2
  mock_service_plan_sku = "B1"
  mock_worker_count     = 2
}

service_capacity = {
  ui_default_capacity   = 1
  ui_minimum_capacity   = 1
  ui_max_capacity       = 10
  api_default_capacity  = 1
  api_minimum_capacity  = 1
  api_max_capacity      = 1
  mock_default_capacity = 1
  mock_minimum_capacity = 1
  mock_max_capacity     = 2
}

subnets = {
  storage   = []
  ui        = []
  api       = []
  endpoints = []
  mock      = []
}