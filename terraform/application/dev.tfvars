environment = {
  name  = "development"
  alias = "dev"
}

terraform_service_principal_display_name = "Azure Pipeline: Complex-Cases-Development"
dns_server                               = "10.7.197.20"
dns_alt_server                           = "168.63.129.16"

service_plans = {
  ui_service_plan_sku         = "B2"
  api_service_plan_sku        = "B3"
  egressMock_service_plan_sku = "B1"
  netAppMock_service_plan_sku = "B1"
}

service_capacity = {
  ui_max_capacity         = "3"
  api_max_capacity        = "1"
  egressMock_max_capacity = "2"
  netAppMock_max_capacity = "2"
}