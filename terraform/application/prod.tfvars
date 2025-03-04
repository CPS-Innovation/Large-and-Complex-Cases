environment = {
  name  = "production"
  alias = "prod"
}

terraform_service_principal_display_name = "Azure Pipeline: Complex-Cases-Production"
dns_server                               = "10.7.204.164"
dns_alt_server                           = "168.63.129.16"

service_plans = {
  ui_service_plan_sku  = "P1mv3"
  api_service_plan_sku = "P1mv3"
}

service_capacity = {
  ui_max_capacity  = "10"
  api_max_capacity = "1"
}