environment = {
  name  = "test"
  alias = "test"
}

terraform_service_principal_display_name = "Azure Pipeline: Complex-Cases-INT"
dns_server                               = "10.7.197.20"
dns_alt_server                           = "168.63.129.16"
subscription_id                          = "7f67e716-03c5-4675-bad2-cc5e28652759"
nsg_name                                 = "basicNsgVNET-LaCC-WANNET-nic01"

service_plans = {
  ui_service_plan_sku   = "B2"
  ui_worker_count       = 2
  api_service_plan_sku  = "B3"
  api_worker_count      = 2
  mock_service_plan_sku = "B1"
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
  storage   = []
  ui        = []
  api       = []
  endpoints = []
  mock      = []
}