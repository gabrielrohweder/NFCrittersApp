import BottomNav from '../BottomNav';
import { Router } from 'wouter';

export default function BottomNavExample() {
  return (
    <Router>
      <div className="h-screen bg-background flex flex-col">
        <div className="flex-1 p-4">
          <p className="text-center text-muted-foreground">
            Navigate between tabs to see the active state
          </p>
        </div>
        <BottomNav />
      </div>
    </Router>
  );
}