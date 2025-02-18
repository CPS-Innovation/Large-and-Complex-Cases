environment = {
  name  = "e2e"
  alias = "e2e"
}

terraform_service_principal_display_name = "Azure Pipeline: Complex-Cases-E2E"
dns_server      = "10.7.197.20"
dns_alt_server  = "168.63.129.16"

service_plans = {
  ui_service_plan_sku = "P1v3"
  api_service_plan_sku = "P1mv3"
  egress_service_plan_sku = "EP2"
  egress_always_ready_instances     = 1
  egress_maximum_scale_out_limit    = 10
  egress_plan_maximum_burst         = 10
  netapp_service_plan_sku = "EP2"
  netapp_always_ready_instances     = 1
  netapp_maximum_scale_out_limit    = 10
  netapp_plan_maximum_burst         = 10
}

api_config = {
  control_queue_buffer_threshold        = 256
  max_concurrent_orchestrator_functions = 325
  max_concurrent_activity_functions     = 325
  max_queue_polling_interval            = "00:00:02"
}

scale_controller_logging = {
  egress = "AppInsights:None"
  netapp = "AppInsights:None"
}