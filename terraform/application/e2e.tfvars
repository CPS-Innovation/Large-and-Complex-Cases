environment = {
  name  = "e2e"
  alias = "e2e"
}

terraform_service_principal_display_name = "Azure Pipeline: Complex-Cases-E2E"
dns_server                               = "10.7.197.20"
dns_alt_server                           = "168.63.129.16"

service_plans = {
  ui_service_plan_sku  = "B2"
  api_service_plan_sku = "B3"
}

service_capacity = {
  ui_max_capacity  = "3"
  api_max_capacity = "1"
}