import random
import numpy as np
import xlsxwriter
import sys


class PSO:
    def __init__(self, n_iter_, n_swarm_, n_, b_, d_, w_, c1_, c2_, database_):
        self.iterations_timer = 0
        self.phi = np.linspace(0, 2 * np.pi, 1000)
        self.phi_180 = [self.phi[i] * (180 / np.pi) for i in range(len(self.phi))]

        self.n_iter = n_iter_
        self.n_swarm = n_swarm_
        self.n = n_
        self.b_bounds = b_
        self.d_bounds = d_
        self.w_bounds = w_
        self.bounds = [b_, d_, w_]

        self.c1 = c1_
        self.c2 = c2_

        self.swarm = []
        self.b = []
        self.d = []
        self.w = []

        self.best_values = []

        self.ans = float
        self.Found_Value = float
        self.Fitness_Value = float

        self.best_ans = float
        self.best_found = float
        self.best_fitness = float
        self.best_sll = []
        self.best_bw = []

        self.dataBase = database_

        self.max_velocity = [b_[0] + b_[1], d_[0] + d_[1], w_[0] + w_[1]]
        self.min_velocity = [-self.max_velocity[0], -self.max_velocity[1], -self.max_velocity[2]]

    class Particles:
        def __init__(self, values_):
            self.Values = values_
            self.BestValues = []
            self.Fitness = 0.0
            self.BestFitness = 0.0
            self.Velocity = []

    def array_factor(self, b_, d_, w_, m_=-60):
        s = 0
        for i in range(self.n):
            psi = (2 * np.pi) * (d_[i]) * (np.cos(self.phi) + b_[i])
            s = s + w_[i] * np.exp(1j * psi * i)
        g = np.abs(s) ** 2
        dbi = 10 * np.log10(g / np.max(g))
        return np.clip(dbi, m_, None)

    def add_value(self):
        self.b = [np.random.uniform(b_bounds[0], b_bounds[1]) for _ in range(self.n)]
        self.d = [np.random.uniform(d_bounds[0], d_bounds[1]) for _ in range(self.n)]
        self.w = [np.random.uniform(w_bounds[0], w_bounds[1]) for _ in range(self.n)]

        return self.b, self.d, self.w

    def update_particles(self):
        r1 = random.random()
        r2 = random.random()

        for x in range(len(self.swarm)):
            for y in range(3):
                for z in range(self.n):
                    self.swarm[x].Velocity[y][z] = self.swarm[x].Velocity[y][z] + \
                                       c1 * r1 * (self.swarm[x].BestValues[y][z] - self.swarm[x].Values[y][z]) + \
                                       c2 * r2 * (self.best_values[y][z] - self.swarm[x].Values[y][z])

                    self.swarm[x].Values[y][z] = self.swarm[x].Values[y][z] + self.swarm[x].Velocity[y][z]

                    if self.swarm[x].Values[y][z] > self.bounds[y][1]:
                        self.swarm[x].Values[y][z] = min(self.swarm[x].Values[y][z], self.bounds[y][1])
                    elif self.swarm[x].Values[y][z] < self.bounds[y][0]:
                        self.swarm[x].Values[y][z] = max(self.swarm[x].Values[y][z], self.bounds[y][0])

    def calculate_fitness(self):
        for Particle in self.swarm:

            self.ans = self.array_factor(Particle.Values[0], Particle.Values[1], Particle.Values[2])

            hp_bw = []
            i = 249
            while i != 0:
                if self.ans[i] < -2.99:
                    hp_bw.append([self.ans[i], (((i + 1) * (2 * np.pi)) / 1000) * (180 / np.pi)])
                    break
                i -= 1

            i = 251
            while i != 500:
                if self.ans[i] < -2.99:
                    hp_bw.append([self.ans[i], (((i + 1) * (2 * np.pi)) / 1000) * (180 / np.pi)])
                    break
                i += 1

            min_dot = 0
            for i in range(251, 500):
                a = self.ans[i]
                b = self.ans[i + 1]
                c = self.ans[i + 2]
                if b <= a and b <= c:
                    if b != a and b != c:
                        min_dot = i + 1
                        break

            sll = [-100, 0]
            for i in range(min_dot, 500):
                if self.ans[i] > sll[0]:
                    sll = [self.ans[i], ((i * (2 * np.pi)) / 1000) * (180 / np.pi)]

            f1 = 0
            for i in range(len(self.ans) - 20):
                f11 = 0
                for j in range(20):
                    f11 = f11 + ((self.phi[j + i + 1] - self.phi[j + i]) * (self.ans[j + i] ** 2))
                f1 = f1 + (f11 / 20)

            f2 = 0
            for i in self.ans:
                f2 = f2 + (i ** 2)

            Particle.Fitness = f1 + f2

            if Particle.Fitness > Particle.BestFitness:
                Particle.BestValues = Particle.Values
                Particle.BestFitness = Particle.Fitness

            if Particle.Fitness > self.Fitness_Value:
                self.best_ans = self.ans
                self.best_values = Particle.Values
                self.Found_Value = Particle.Values
                self.Fitness_Value = Particle.Fitness
                self.best_sll = sll
                self.best_bw = hp_bw

    def main(self):
        self.b = [np.random.uniform(b_bounds[1], b_bounds[1]) for _ in range(self.n)]
        self.d = [np.random.uniform(d_bounds[1], d_bounds[1]) for _ in range(self.n)]
        self.w = [np.random.uniform(w_bounds[1], w_bounds[1]) for _ in range(self.n)]

        self.swarm.append(self.Particles([self.b, self.d, self.w]))

        for i in range(self.n_swarm - 1):
            self.swarm.append(self.Particles(self.add_value()))

        for i in self.swarm:
            i.BestValues = i.Values

        for i in self.swarm:
            i.Velocity = [[random.random() * (self.max_velocity[i] - self.min_velocity[i]) + self.min_velocity[i] for _ in range(self.n)] for i in range(3)]

        self.best_fitness = 0.0
        self.Fitness_Value = 0.0

        while self.iterations_timer != self.n_iter:
            self.calculate_fitness()
            self.swarm = sorted(self.swarm, key=lambda Particles: Particles.Fitness)
            self.update_particles()
            self.iterations_timer += 1
            if self.Fitness_Value > self.best_fitness:
                self.best_fitness = self.Fitness_Value
                self.best_found = self.Found_Value

        workbook = xlsxwriter.Workbook(dataBase + '/arrayfactor_pso.xlsx')
        page = workbook.add_worksheet()

        page.write(0, 1, 'Parameters')
        page.write(1, 0, 'n_iter')
        page.write(2, 0, 'n_swarm')
        page.write(3, 0, 'N')
        page.write(4, 0, 'b_bounds')
        page.write(5, 0, 'd_bounds')
        page.write(6, 0, 'w_bounds')
        page.write(7, 0, 'c1')
        page.write(8, 0, 'c2')

        page.write(1, 1, self.n_iter)
        page.write(2, 1, self.n_swarm)
        page.write(3, 1, self.n)
        page.write(4, 1, self.b_bounds[0])
        page.write(4, 2, self.b_bounds[1])
        page.write(5, 1, self.d_bounds[0])
        page.write(5, 2, self.d_bounds[1])
        page.write(6, 1, self.w_bounds[0])
        page.write(6, 2, self.w_bounds[1])
        page.write(7, 1, self.c1)
        page.write(8, 1, self.c2)

        page.write(0, 3, 'B Bounds')
        page.write(0, 4, 'D Bounds')
        page.write(0, 5, 'W Bounds')
        page.write(0, 6, 'Band Width')
        page.write(0, 7, 'SLL')
        page.write(0, 8, 'Degrees')
        page.write(0, 9, 'Best Answers')

        page.write(1, 6, (self.best_bw[1][1] - self.best_bw[0][1]))
        page.write(1, 7, self.best_sll[0])

        page.write_column('D2', self.best_values[0])
        page.write_column('E2', self.best_values[1])
        page.write_column('F2', self.best_values[2])
        page.write_column('I2', self.phi_180)
        page.write_column('J2', self.best_ans)

        graph = workbook.add_chart({'type': 'line'})
        graph.add_series({'name': 'AF', 'categories': '= Sheet1!I2:I502', 'values': '= Sheet1!J2:J502'})

        graph.set_title({'name': 'AF'})
        graph.set_x_axis({'name': 'Theta (degree)', 'min': '0', 'max': '180'})
        graph.set_y_axis({'name': 'Array Factor (dB)', 'min': '-60', 'max': '0'})
        graph.set_style(11)

        page.insert_chart('L2', graph, {'x_scale': 2, 'y_scale': 2})

        workbook.close()


n_iter = int(sys.argv[1])
n_swarm = int(sys.argv[2])

N = int(sys.argv[3])
b_bounds = [float(sys.argv[4]), float(sys.argv[5])]
d_bounds = [float(sys.argv[6]), float(sys.argv[7])]
w_bounds = [float(sys.argv[8]), float(sys.argv[9])]

c1 = float(sys.argv[10])
c2 = float(sys.argv[11])

dataBase = sys.argv[12]

Go = PSO(n_iter, n_swarm, N, b_bounds, d_bounds, w_bounds, c1, c2, dataBase)
Go.main()
