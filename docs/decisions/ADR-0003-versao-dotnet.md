# ADR-0003 — Versão do .NET e Runtime Oficial

- **Status:** ACCEPTED
- **Date:** 2026-11-01
- **Decision Owner:** Engenheiro / Arquiteto
- **Technical Context:** Fase 02 - Arquitetura e Design

---

## 1. Context (Contexto e Problema)
A aplicação necessita de uma plataforma moderna, com suporte a longo prazo (LTS), alta performance para I/O assíncrono e suporte universal em contêineres Docker leves em Linux.

## 2. Options Considered (Alternativas Avaliadas)
- **Opção 1 (.NET 8.0 LTS):** Suporte estendido da Microsoft, alta estabilidade, maduro no ecossistema corporativo.
- **Opção 2 (.NET 9.0 STS):** Últimos recursos, porém ciclo de vida curto (Standard Term Support).

## 3. Decision (Decisão)
Adotar **.NET 8.0 (LTS)** como target framework oficial (`net8.0`) em todos os projetos da solution (`AgendaPalestras.Domain`, `AgendaPalestras.Application`, `AgendaPalestras.Infrastructure`, `AgendaPalestras.Web` e projetos de teste). O SDK utilizado no build local e CI é compatível com .NET 8 e superior.

## 4. Consequences (Consequências)
- **Positivas:** Estabilidade garantida, suporte oficial estendido, imagens Docker `mcr.microsoft.com/dotnet/aspnet:8.0` enxutas e otimizadas.
- **Negativas:** Recursos exclusivos do C# 13/.NET 9 não serão utilizados na fase inicial.
