//
//  LSystem.cpp
//  burningbush
//
//  Created by Dewb on 12/26/14.
//
//

#include "LSystem.h"
#include "LSystemRulesEngine.h"
#include <iostream>

#ifdef USE_CXX_TR1
#include <tr1/random>
typedef tr1::ranlux_base_01 random_number_generator_t;
typedef tr1::uniform_real<float> distribution_t;
#else
#include <random>
typedef std::ranlux24_base random_number_generator_t;
typedef std::uniform_real_distribution<float> distribution_t;
#endif

LSystem::LSystem() {
    seed = abs(rand());
}

void LSystem::setAxiom(const RuleString& axiomString) {
    axiom = axiomString;
}

void LSystem::ignoreForContext(const RuleString& ignoreString) {
    ignoreContext = ignoreString;
}

ProductionRule& LSystem::addRule(const RuleToken& predeccessor, const RuleString& successor) {
    ProductionRule rule(predeccessor, successor);
    
    if (rules.find(predeccessor) == rules.end()) {
        rules.insert(pair<RuleToken, ProductionRuleGroup>(predeccessor, ProductionRuleGroup()));
    }
    
    rules[predeccessor].push_back(rule);
    return rules[predeccessor].back();
}

ProductionRule& LSystem::addRule(const RuleString& leftContext, const RuleToken& predecessor, const RuleString& rightContext, const RuleString& successor) {
    
    ProductionRule& rule = addRule(predecessor, successor);
    rule.leftContext = leftContext;
    rule.rightContext = rightContext;
    return rule;
}

void LSystem::reset() {
    rules.clear();
    properties.clear();
    axiom.clear();
    title = "";
}

void LSystem::reseed(unsigned newSeed) {
    seed = newSeed;
}

void LSystem::reseed() {
    reseed(abs(rand()));
}

bool LSystem::isStochastic() const {
    for (auto& rule : rules) {
        if (rule.second.isStochastic()) {
            return true;
        }
    }
    return false;
}

typedef pair<RuleString::iterator, RuleString> Replacement;
typedef vector<Replacement> Replacements;

RuleString LSystem::generate(int iterations, bool logging) {
    RuleString current = axiom;
    
    random_number_generator_t generator(seed);
    distribution_t distribution(0.0, 1.0);

    IndexedProductionRuleGroup matchedRules;
    Replacements replacements;
    LSystemRulesEngine engine(this);
    
    if (logging) {
        cout << current << "\n";
    }

    while (iterations--) {
        replacements.clear();
        auto currentPos = current.begin();
        while(currentPos != current.end()) {
            matchedRules.clear();
            engine.getMatchingRules(current, currentPos, matchedRules);
            if (matchedRules.size()) {
                if (matchedRules.size() == 1) {
                    // Basic case: just one matching rule
                    auto& rule = matchedRules[0].second;
                    int ruleIndex = matchedRules[0].first;
                    if (logging) {
                        cout << "Executing: " << rule << "\n";
                    }
                    RuleString successor =
                        engine.evaluateSuccessor(ruleIndex, rule.predecessor, *currentPos, rule.successor);
                    replacements.push_back(Replacement(currentPos, successor));
                } else {
                    // Stochastic case: multiple rules
                    float totalProbability = 0;
                    for (auto& item : matchedRules) {
                        totalProbability += item.second.probability;
                    }
                    float d = distribution(generator);
                    float p = d * totalProbability;
                    float s = 0;
                    for (auto iter = matchedRules.rbegin(); iter != matchedRules.rend(); ++iter) {
                        auto& rule = iter->second;
                        int ruleIndex = iter->first;
                        s += rule.probability;
                        if (s > p) {
                            if (logging) {
                                cout << "Executing: " << rule << "\n";
                            }
                            RuleString successor =
                                engine.evaluateSuccessor(ruleIndex, rule.predecessor, *currentPos, rule.successor);
                            replacements.push_back(Replacement(currentPos, successor));
                            break;
                        }
                    }
                }
            }
            ++currentPos;
        }
        
        for (auto& repl : replacements) {
            current.insert(current.erase(repl.first), repl.second.begin(), repl.second.end());
        }
        
        if (logging) {
            cout << current << "\n";
        }
    }
    return current;
}

float LSystem::getProperty(string name) const {
    auto iter = properties.find(name);
    if (iter != properties.end()) {
        return iter->second;
    } else {
        return 0;
    }
}

void LSystem::setProperty(string name, float value) {
    properties[name] = value;
}

bool LSystem::hasProperty(string name) const {
    return properties.find(name) != properties.end();
}