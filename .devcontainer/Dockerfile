FROM mcr.microsoft.com/devcontainers/dotnet:9.0-noble

# Install required tools and dependencies
RUN apt-get update && apt-get install -y \
    curl \
    apt-transport-https \
    ca-certificates \
    lsb-release \
    software-properties-common \
    unzip \
    wget \
    && rm -rf /var/lib/apt/lists/*

# Add the Zscaler public certificates (CA and intermediates) to the image
# If Zscaler is not used, then no harm is done.
COPY  certs/Zscaler_Intermediate_Root_CA_zscaler.net.crt \
      certs/Zscaler_Intermediate_Root_CA_zscaler.net_t_until_06_sept_2025.crt \
      certs/Zscaler_Root_CA.crt \
      /usr/local/share/ca-certificates/

# Update the CA certificates
RUN update-ca-certificates

# Add dotnet8 SDK in a 'shared root' (the same folder as the dotnet 9 SDK)
RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh \
    && chmod +x dotnet-install.sh \
    && ./dotnet-install.sh --version 8.0.100 --install-dir /usr/share/dotnet

# Set environment variables
#ENV DOTNET_ROOT=/usr/share/dotnet

# Add Microsoft package feed, and azure function core tools for noble (Ubuntu 24.04)
RUN wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y azure-functions-core-tools
