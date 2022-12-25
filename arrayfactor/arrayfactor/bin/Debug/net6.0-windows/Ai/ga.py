import random
import numpy as np
import xlsxwriter
import sys


class Genetic:
    def __init__(self, n_bits_, n_iter_, n_pop_, r_cross_, r_mut_, n_, b_, d_, w_, database_):
        self.iterations_timer = 0
        self.phi = np.linspace(0, 2 * np.pi, 1000)
        self.phi_180 = [self.phi[i] * (180 / np.pi) for i in range(len(self.phi))]

        self.n_bits = n_bits_
        self.n_iter = n_iter_
        self.n_pop = n_pop_
        self.r_cross = r_cross_
        self.r_mut = r_mut_
        self.n = n_
        self.b_bounds = b_
        self.d_bounds = d_
        self.w_bounds = w_

        self.pop = []
        self.Next_Generation = []
        self.last_best = []
        self.member_1 = []
        self.member_2 = []
        self.new_member = []
        self.b = []
        self.d = []
        self.w = []

        self.ans = float
        self.Found_Value = float
        self.Fitness_Value = float

        self.best_ans = float
        self.best_found = float
        self.best_fitness = float
        self.best_decoded = []
        self.best_sll = []
        self.best_bw = []

        self.dataBase = database_

    class Member:
        def __init__(self, chromosome_):
            self.Chromosome = chromosome_
            self.Fitness = 0
            self.s_ratio = 0

    def decode(self, bounds_, n_bits_, bitstring_):
        decoded = list()
        largest = 2 ** n_bits_
        start, end = 0, n_bits_
        for i in bitstring_:
            substring = i[start:end]
            chars = ''.join([str(s) for s in substring])
            integer = int(chars, 2)
            value = bounds_[0] + (integer / largest) * (bounds_[1] - bounds_[0])
            decoded.append(value)
        return decoded

    def array_factor(self, b_, d_, w_, m_=-60):
        s = 0
        for i in range(self.n):
            psi = (2 * np.pi) * (d_[i]) * (np.cos(self.phi) + b_[i])
            s = s + w_[i] * np.exp(1j * psi * i)
        g = np.abs(s) ** 2
        dbi = 10 * np.log10(g / np.max(g))
        return np.clip(dbi, m_, None)

    def random_gene(self):
        gene = random.randint(0, 1)
        return gene

    def create_chromosome(self):
        self.b = [[self.random_gene() for _ in range(n_bits)] for _ in range(self.n)]
        self.d = [[self.random_gene() for _ in range(n_bits)] for _ in range(self.n)]
        self.w = [[self.random_gene() for _ in range(n_bits)] for _ in range(self.n)]

        return self.b, self.d, self.w

    def calculate_fitness(self):
        for Member in self.pop:
            decoded = []

            i = 0
            for member in Member.Chromosome:
                if i == 0:
                    decoded.extend([self.decode(self.b_bounds, self.n_bits, member)])
                if i == 1:
                    decoded.extend([self.decode(self.d_bounds, self.n_bits, member)])
                if i == 2:
                    decoded.extend([self.decode(self.w_bounds, self.n_bits, member)])
                i += 1

            self.ans = self.array_factor(decoded[0], decoded[1], decoded[2])

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
                    f11 = f11 + ((self.phi[j+i+1] - self.phi[j+i]) * (self.ans[j+i]**2))
                f1 = f1 + (f11 / 20)

            f2 = 0
            for i in self.ans:
                f2 = f2 + (i**2)

            Member.Fitness = f1 + f2

            if Member.Fitness > self.Fitness_Value:
                self.best_ans = self.ans
                self.Found_Value = Member.Chromosome
                self.Fitness_Value = Member.Fitness
                self.best_sll = sll
                self.best_bw = hp_bw
                self.best_decoded = decoded

    def selection(self):
        total_fitness = 0.0
        total_ratio = 100.0

        for Member in self.Next_Generation:
            total_fitness += Member.Fitness

        for Member in self.Next_Generation:
            Member.s_ratio = (Member.Fitness / total_fitness) * 100

        pt = random.randint(0, 100)
        for Member in self.Next_Generation:
            total_ratio -= Member.s_ratio
            if total_ratio <= pt:
                self.member_1 = Member.Chromosome
                break

        pt = random.randint(0, 100)
        for Member in self.Next_Generation:
            total_ratio -= Member.s_ratio
            if total_ratio <= pt:
                self.member_2 = Member.Chromosome
                break

    def crossover(self, r_cross_):
        self.last_best = int((90 * self.n_pop) / 100)

        self.Next_Generation = []
        self.Next_Generation.extend(self.pop[self.last_best:])

        while True:
            if len(self.Next_Generation) < len(self.pop):
                self.new_member = []
                self.selection()
                if random.random() < r_cross_:
                    for i in range(3):
                        c1 = []
                        for j in range(self.n):
                            pt = random.randint(1, n_bits - 2)
                            c1.append(self.member_1[i][j][:pt] + self.member_2[i][j][pt:])
                        self.new_member.append(c1)

                    self.new_member = tuple(self.new_member)

                else:
                    if random.random() < 0.5:
                        self.new_member.extend(self.member_1)
                    else:
                        self.new_member.extend(self.member_2)

                self.new_member = self.mutation(self.new_member, self.r_mut)

                self.Next_Generation.append(self.Member(self.new_member))

            else:
                break

        self.pop = self.Next_Generation

    def mutation(self, bitstring_, r_mut_):
        for i in range(len(bitstring_)):
            for j in range(len(bitstring_[i])):
                for k in range(len(bitstring_[i][j])):
                    if random.random() < r_mut_:
                        bitstring_[i][j][k] = 1 - bitstring_[i][j][k]
        return bitstring_

    def genetic_algorithm(self):
        for i in range(self.n_pop):
            self.pop.append(self.Member(self.create_chromosome()))

        self.best_fitness = 0.0
        self.Fitness_Value = 0.0

        while self.iterations_timer != self.n_iter:
            self.calculate_fitness()
            self.pop = sorted(self.pop, key=lambda Member: Member.Fitness)
            self.crossover(self.r_cross)
            self.iterations_timer += 1
            if self.Fitness_Value > self.best_fitness:
                self.best_fitness = self.Fitness_Value
                self.best_found = self.Found_Value

        workbook = xlsxwriter.Workbook(dataBase + '/arrayfactor_ga.xlsx')
        page = workbook.add_worksheet()

        page.write(0, 1, 'Parameters')
        page.write(1, 0, 'n_iter')
        page.write(2, 0, 'n_bits')
        page.write(3, 0, 'n_pop')
        page.write(4, 0, 'r_cross')
        page.write(5, 0, 'r_mut')
        page.write(6, 0, 'N')
        page.write(7, 0, 'b_bounds')
        page.write(8, 0, 'd_bounds')
        page.write(9, 0, 'w_bounds')

        page.write(1, 1, self.n_iter)
        page.write(2, 1, self.n_bits)
        page.write(3, 1, self.n_pop)
        page.write(4, 1, self.r_cross)
        page.write(5, 1, self.r_mut)
        page.write(6, 1, self.n)
        page.write(7, 1, self.b_bounds[0])
        page.write(7, 2, self.b_bounds[1])
        page.write(8, 1, self.d_bounds[0])
        page.write(8, 2, self.d_bounds[1])
        page.write(9, 1, self.w_bounds[0])
        page.write(9, 2, self.w_bounds[1])

        page.write(0, 3, 'B Bounds')
        page.write(0, 4, 'D Bounds')
        page.write(0, 5, 'W Bounds')
        page.write(0, 6, 'Band Width')
        page.write(0, 7, 'SLL')
        page.write(0, 8, 'Degrees')
        page.write(0, 9, 'Best Answers')

        page.write(1, 6, (self.best_bw[1][1]-self.best_bw[0][1]))
        page.write(1, 7, self.best_sll[0])

        page.write_column('D2', self.best_decoded[0])
        page.write_column('E2', self.best_decoded[1])
        page.write_column('F2', self.best_decoded[2])
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
n_bits = int(sys.argv[2])
n_pop = int(sys.argv[3])
r_cross = float(sys.argv[4])
r_mut = 1.0 / float(n_bits)

N = int(sys.argv[5])
b_bounds = [float(sys.argv[6]), float(sys.argv[7])]
d_bounds = [float(sys.argv[8]), float(sys.argv[9])]
w_bounds = [float(sys.argv[10]), float(sys.argv[11])]

dataBase = sys.argv[12]

Go = Genetic(n_bits, n_iter, n_pop, r_cross, r_mut, N, b_bounds, d_bounds, w_bounds, dataBase)
Go.genetic_algorithm()
