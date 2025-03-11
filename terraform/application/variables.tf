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
    ui_service_plan_sku         = string
    ui_worker_count             = number
    api_service_plan_sku        = string
    api_worker_count            = number
    egressMock_service_plan_sku = string
    egressMock_worker_count     = number
    netAppMock_service_plan_sku = string
    netAppMock_worker_count     = number
  })
}

variable "service_capacity" {
  type = object({
    ui_default_capacity         = number
    ui_minimum_capacity         = number
    ui_max_capacity             = number
    api_default_capacity        = number
    api_minimum_capacity        = number
    api_max_capacity            = number
    egressMock_default_capacity = number
    egressMock_minimum_capacity = number
    egressMock_max_capacity     = number
    netAppMock_default_capacity = number
    netAppMock_minimum_capacity = number
    netAppMock_max_capacity     = number
  })
}

variable "dns_server" {
  type = string
}

variable "dns_alt_server" {
  type = string
}