#!/bin/bash
set -e

NAMESPACE="testservice"
DEPLOYMENT_PATH="deployment/kubernetes"

echo "Deploying TestService to Kubernetes..."

# Create namespace if it doesn't exist
kubectl apply -f $DEPLOYMENT_PATH/namespace.yaml

# Apply ConfigMap and Secrets
kubectl apply -f $DEPLOYMENT_PATH/configmap.yaml

# Apply Deployment
kubectl apply -f $DEPLOYMENT_PATH/deployment.yaml

# Apply Service
kubectl apply -f $DEPLOYMENT_PATH/service.yaml

# Apply Ingress
kubectl apply -f $DEPLOYMENT_PATH/ingress.yaml

# Apply HPA
kubectl apply -f $DEPLOYMENT_PATH/hpa.yaml

# Apply PDB
kubectl apply -f $DEPLOYMENT_PATH/pdb.yaml

echo "Waiting for deployment to be ready..."
kubectl wait --for=condition=available --timeout=300s deployment/testservice-api -n $NAMESPACE

echo "Deployment completed successfully!"
echo "Service URL: http://testservice.example.com"