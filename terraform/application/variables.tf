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
    ui_service_plan_sku  = string
    api_service_plan_sku = string
  })
}

variable "service_capacity" {
  type = object({
    ui_max_capacity  = string
    api_max_capacity = string
  })
}

variable "dns_server" {
  type = string
}

variable "dns_alt_server" {
  type = string
}