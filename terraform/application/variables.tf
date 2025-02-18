variable "environment" {
  type = object({
    name  = string
    alias = string
  })
}

variable "location" {
  default = "UK South"
}

variable "terraform_service_principal_display_name" {
  type = string
}

variable "service_plans" {
  type = object({
    ui_service_plan_sku = string
    api_service_plan_sku = string
    egress_service_plan_sku = string
    egress_always_ready_instances     = number
    egress_maximum_scale_out_limit    = number
    egress_plan_maximum_burst         = number
    netapp_service_plan_sku = string
    netapp_always_ready_instances     = number
    netapp_maximum_scale_out_limit    = number
    netapp_plan_maximum_burst         = number
  })
}

variable "dns_server" {
  type = string
}

variable "dns_alt_server" {
  type = string
}

variable "api_config" {
  type = object({
    control_queue_buffer_threshold        = number
    max_concurrent_activity_functions     = number
    max_concurrent_orchestrator_functions = number
    max_queue_polling_interval            = string #hh:mm:ss format e.g. "00:00:05" for 5 seconds
  })
}

variable "scale_controller_logging" {
  type = object({
    egress  = string
    netapp = string
  })
}