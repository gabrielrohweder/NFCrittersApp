import { Link, useLocation } from "wouter";
import { motion } from "framer-motion";
import { Home, Grid3x3, Zap, User } from "lucide-react";

interface NavItem {
  path: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
}

const navItems: NavItem[] = [
  { path: "/", label: "Scan", icon: Zap },
  { path: "/collection", label: "Collection", icon: Grid3x3 },
  { path: "/profile", label: "Profile", icon: User },
];

export default function BottomNav() {
  const [location] = useLocation();

  return (
    <motion.nav 
      className="fixed bottom-0 left-0 right-0 bg-card border-t border-card-border z-40"
      initial={{ y: 100 }}
      animate={{ y: 0 }}
      transition={{ type: "spring", stiffness: 300, damping: 30 }}
    >
      <div className="flex items-center justify-around px-4 py-2">
        {navItems.map((item) => {
          const isActive = location === item.path;
          const Icon = item.icon;
          
          return (
            <Link key={item.path} href={item.path}>
              <motion.div
                className={`flex flex-col items-center gap-1 p-2 rounded-lg transition-colors relative ${
                  isActive 
                    ? "text-primary" 
                    : "text-muted-foreground hover:text-foreground"
                }`}
                whileTap={{ scale: 0.95 }}
                data-testid={`nav-${item.label.toLowerCase()}`}
              >
                {isActive && (
                  <motion.div
                    className="absolute inset-0 bg-primary/10 rounded-lg"
                    layoutId="activeTab"
                    transition={{ type: "spring", stiffness: 500, damping: 30 }}
                  />
                )}
                <Icon className={`w-6 h-6 relative z-10 ${isActive ? "text-primary" : ""}`} />
                <span className={`text-xs font-medium relative z-10 ${isActive ? "text-primary" : ""}`}>
                  {item.label}
                </span>
              </motion.div>
            </Link>
          );
        })}
      </div>
    </motion.nav>
  );
}