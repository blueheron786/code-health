
package com.codehealth.scanners.java;

import java.util.*;
import java.io.*;
import java.nio.file.*;
import java.util.regex.*;
import org.json.simple.JSONArray;
import org.json.simple.JSONObject;

public class CyclomaticComplexityScanner {
    public static void main(String[] args) {
        if (args.length != 2) {
            System.out.println("Usage: java CyclomaticComplexityScanner <sourceRootPath> <outputFile>");
            return;
        }

        String sourceRoot = args[0];
        String outputFile = args[1];

        JSONObject report = new JSONObject();
        JSONArray files = new JSONArray();
        int totalComplexity = 0;
        int totalMethods = 0;

        try {
            Files.walk(Paths.get(sourceRoot))
                .filter(Files::isRegularFile)
                .filter(p -> p.toString().endsWith(".java"))
                .forEach(filePath -> {
                    JSONObject fileJson = new JSONObject();
                    JSONArray methodsArray = new JSONArray();
                    String relativePath = sourceRoot.relativize(filePath.toAbsolutePath()).toString().replace("\\", "/");
                    fileJson.put("File", relativePath);

                    try {
                        List<String> lines = Files.readAllLines(filePath);
                        StringBuilder methodBody = new StringBuilder();
                        boolean inMethod = false;
                        int complexity = 1;
                        String methodName = null;

                        for (String line : lines) {
                            line = line.trim();
                            if (line.matches(".*\b(public|private|protected)?\s+\w+\s+\w+\(.*\)\s*\{.*")) {
                                inMethod = true;
                                methodName = line.split("\(")[0].replaceAll(".*\s", "");
                                complexity = 1;
                                continue;
                            }
                            if (inMethod) {
                                methodBody.append(line).append("\n");

                                if (line.contains("{")) complexity++;
                                if (line.contains("if") || line.contains("for") || line.contains("while")
                                        || line.contains("case") || line.contains("catch")
                                        || line.contains("&&") || line.contains("||") || line.contains("?")) {
                                    complexity++;
                                }

                                if (line.contains("}")) {
                                    JSONObject methodJson = new JSONObject();
                                    methodJson.put("Method", methodName);
                                    methodJson.put("Complexity", complexity);
                                    methodsArray.add(methodJson);
                                    totalComplexity += complexity;
                                    totalMethods++;
                                    inMethod = false;
                                }
                            }
                        }

                        if (!methodsArray.isEmpty()) {
                            fileJson.put("Methods", methodsArray);
                            files.add(fileJson);
                        }
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                });

            report.put("Files", files);
            report.put("TotalComplexity", totalComplexity);
            report.put("AverageComplexity", totalMethods > 0 ? (double) totalComplexity / totalMethods : 0.0);

            try (FileWriter writer = new FileWriter(outputFile)) {
                writer.write(report.toJSONString());
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}
