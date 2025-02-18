environment = {
  name  = "production"
  alias = "prod"
}

terraform_service_principal_display_name = "Azure Pipeline: Complex-Cases-Production"
dns_server                               = "10.7.204.164"
dns_alt_server                           = "168.63.129.16"

service_plans = {
  ui_service_plan_sku = "P1v3"
  api_service_plan_sku = "P3mv3"
  egress_service_plan_sku = "EP3"
  egress_always_ready_instances     = 3
  egress_maximum_scale_out_limit    = 15
  egress_plan_maximum_burst         = 15
  netapp_service_plan_sku = "EP3"
  netapp_always_ready_instances     = 3
  netapp_maximum_scale_out_limit    = 15
  netapp_plan_maximum_burst         = 15
}

api_config = {
  control_queue_buffer_threshold        = 1000
  max_concurrent_orchestrator_functions = 1000
  max_concurrent_activity_functions     = 1000
  max_queue_polling_interval            = "00:00:02"
}

scale_controller_logging = {
  egress = "AppInsights:None"
  netapp = "AppInsights:None"
}