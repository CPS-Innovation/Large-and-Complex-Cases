environment = {
  name  = "production"
  alias = "prod"
}

terraform_service_principal_display_name = "Azure Pipeline: Complex-Cases-Production"
dns_server                               = "10.7.204.164"
dns_alt_server                           = "168.63.129.16"

service_plans = {
  ui_service_plan_sku         = "P1mv3"
  ui_worker_count             = 2
  api_service_plan_sku        = "P1mv3"
  api_worker_count            = 2
  egressMock_service_plan_sku = "B1"
  egressMock_worker_count     = 2
  netAppMock_service_plan_sku = "B1"
  netAppMock_worker_count     = 2
}

service_capacity = {
  ui_default_capacity         = 1
  ui_minimum_capacity         = 1
  ui_max_capacity             = 10
  api_default_capacity        = 1
  api_minimum_capacity        = 1
  api_max_capacity            = 1
  egressMock_default_capacity = 1
  egressMock_minimum_capacity = 1
  egressMock_max_capacity     = 2
  netAppMock_default_capacity = 1
  netAppMock_minimum_capacity = 1
  netAppMock_max_capacity     = 2
}

subnets = {
  storage    = []
  ui         = []
  api        = []
  endpoints  = []
  egressMock = []
  netAppMock = []
}