# Readme

## K8s
**Sprint up instances**
```
kubectl apply -f .k8s/namespace.yaml & kubectl apply -f .k8s/deployment.yaml & kubectl apply -f .k8s/service.yaml & kubectl apply -f .k8s/ingress.yaml 
```

## Docker

**Build Image**

```
 docker build -t certpoc:latest --no-cache .
```

**Run Container**

```
docker run --name cert_poc -e ASPNETCORE_ENVIRONMENT=Container -e ASPNETCORE_AZ_KV_URI={{ASPNETCORE_AZ_KV_URI}} -e ASPNETCORE_AZ_TENENT={{ASPNETCORE_AZ_TENENT}} -e ASPNETCORE_AZ_CLIENT_ID={{ASPNETCORE_AZ_CLIENT_ID}} -e ASPNETCORE_AZ_CLIENT_SECRET={{ASPNETCORE_AZ_CLIENT_SECRET}} -e ASPNETCORE_AZ_KV_SECRET_NAME={{ASPNETCORE_AZ_KV_SECRET_NAME}} -e ASPNETCORE_AZ_KV_CERT_NAME={{ASPNETCORE_AZ_KV_CERT_NAME}} -p 7212:7212 -d certpoc:latest
```

## SSL Certificate Setup
**Create Cert Key**
```
openssl genrsa -out ddrpayment.modernapp.bbl.key 4096
```
**Create Cert Request .csr**
```
openssl req -new -days 365 -key ddrpayment.modernapp.bbl.key -out ddrpayment.modernapp.bbl.csr
```
**Create Cert .crt**
```
openssl x509 -days 365 -req -in ddrpayment.modernapp.bbl.csr -out ddrpayment.modernapp.bbl.crt -signkey ddrpayment.modernapp.bbl.key
```
**Create Cert .pfx**
```
openssl pkcs12 -export -in ddrpayment.modernapp.bbl.crt -inkey ddrpayment.modernapp.bbl.key -out ddrpayment.modernapp.bbl.pfx
```
