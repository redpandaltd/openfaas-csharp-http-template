FROM openfaas/of-watchdog:0.8.1 as watchdog

FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder

# Supress collection of data.
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# Optimize for Docker builder caching by adding projects first.

RUN mkdir -p /home/app/function
WORKDIR /home/app/src/function
COPY ./function/Function.csproj  .

WORKDIR /home/app/src/root
COPY ./root/root.csproj  .
RUN dotnet restore ./root.csproj

# Copy the rest of the code
WORKDIR /home/app/src
COPY .  .

RUN dotnet publish -c release -o published ./root/root.csproj

FROM mcr.microsoft.com/dotnet/aspnet:5.0

COPY --from=watchdog /fwatchdog /usr/bin/

# Create a non-root user
RUN addgroup --system app \
    && adduser --system --ingroup app app

WORKDIR /home/app/
COPY --from=builder /home/app/src/published .
RUN chown app:app -R /home/app

USER app

ENV fprocess="dotnet ./root.dll"
ENV cgi_headers="true"
ENV mode="http"
ENV upstream_url="http://localhost:5000"
EXPOSE 8080

HEALTHCHECK --interval=3s CMD [ -e /tmp/.lock ] || exit 1

CMD ["fwatchdog"]
