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

variable "subscription_id" {
  type = string
}

variable "service_plans" {
  type = object({
    ui_service_plan_sku   = string
    ui_worker_count       = number
    api_service_plan_sku  = string
    api_worker_count      = number
    mock_service_plan_sku = string
    mock_worker_count     = number
  })
}

variable "service_capacity" {
  type = object({
    ui_default_capacity   = number
    ui_minimum_capacity   = number
    ui_max_capacity       = number
    api_default_capacity  = number
    api_minimum_capacity  = number
    api_max_capacity      = number
    mock_default_capacity = number
    mock_minimum_capacity = number
    mock_max_capacity     = number
  })
}

variable "dns_server" {
  type = string
}

variable "dns_alt_server" {
  type = string
}

variable "subnets" {
  type = object({
    storage     = list(string)
    ui          = list(string)
    api         = list(string)
    endpoints   = list(string)
    mock        = list(string)
    buildAgents = list(string)
  })
}
